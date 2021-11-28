﻿namespace SqlHydra.Query

open System.Data.Common
open System.Threading
open SqlKata

/// Contains methods that compile and read a query.
type QueryContext(conn: DbConnection, compiler: SqlKata.Compilers.Compiler) =
    let setProviderDbType (param: DbParameter) (propertyName: string) (providerDbType: string) =
        let property = param.GetType().GetProperty(propertyName)
        let dbTypeSetter = property.GetSetMethod()
        
        let value = System.Enum.Parse(property.PropertyType, providerDbType)
        dbTypeSetter.Invoke(param, [|value|]) |> ignore
        
    let setParameterDbType (param: DbParameter) (qp: QueryParameter) =
        match qp.ProviderDbType, compiler with
        | Some type', :? SqlKata.Compilers.PostgresCompiler ->
            setProviderDbType param "NpgsqlDbType" type'
        | Some type', :? SqlKata.Compilers.SqlServerCompiler ->
            setProviderDbType param "SqlDbType" type'
        | _ -> ()    
        
    interface System.IDisposable with
        member this.Dispose() = 
            conn.Dispose()
            this.Transaction |> Option.iter (fun t -> t.Dispose())
            this.Transaction <- None

    member this.Connection = conn
    member this.Compiler = compiler

    member val Transaction : DbTransaction option = None with get,set

    member this.BeginTransaction(?isolationLevel: System.Data.IsolationLevel) = 
        this.Transaction <- 
            match isolationLevel with
            | Some il -> conn.BeginTransaction(il) |> Some
            | None -> conn.BeginTransaction() |> Some

    member this.CommitTransaction() = 
        match this.Transaction with
        | Some t -> t.Commit(); this.Transaction <- None
        | None -> failwith "No transaction was started."

    member this.RollbackTransaction() =
        match this.Transaction with
        | Some t -> t.Rollback(); this.Transaction <- None
        | None -> failwith "No transaction was started."

    member private this.TrySetTransaction(cmd: DbCommand) =
        this.Transaction |> Option.iter (fun t -> cmd.Transaction <- t)

    /// Builds a DbCommand with CommandText and Parameters from a SqlKata compiled query.
    member this.BuildCommand(compiledQuery: SqlResult) =        
        let cmd = conn.CreateCommand()
        cmd |> this.TrySetTransaction
        cmd.CommandText <- compiledQuery.Sql
        for kvp in compiledQuery.NamedBindings do
            let p = cmd.CreateParameter()
            p.ParameterName <- kvp.Key

            match kvp.Value with
            | :? QueryParameter as qp ->
                do setParameterDbType p qp
                p.Value <- qp.Value
            | _ ->
                p.Value <- kvp.Value
            cmd.Parameters.Add(p) |> ignore
        cmd

    member this.BuildCommand(query: Query) =
        let compiledQuery = query |> compiler.Compile
        this.BuildCommand(compiledQuery)

    member this.GetReader<'T, 'Reader when 'Reader :> DbDataReader> (query: SelectQuery<'T>) = 
        let cmd = this.BuildCommand(query.ToKataQuery()) // do not dispose cmd
        cmd.ExecuteReader() :?> 'Reader

    member this.GetReaderAsync<'T, 'Reader when 'Reader :> DbDataReader> (query: SelectQuery<'T>) = 
        this.GetReaderAsync<'T, 'Reader>(query, CancellationToken.None)

    member this.GetReaderAsync<'T, 'Reader when 'Reader :> DbDataReader> (query: SelectQuery<'T>, cancellationToken: CancellationToken) = 
        async {
            let cmd = this.BuildCommand(query.ToKataQuery()) // do not dispose cmd
            let! reader = cmd.ExecuteReaderAsync(cancellationToken) |> Async.AwaitTask
            return reader :?> 'Reader
        }
        |> Async.StartImmediateAsTask

    member this.Read<'Entity, 'Reader when 'Reader :> DbDataReader> (getReaders: 'Reader -> (unit -> 'Entity)) (query: SelectQuery<'Entity>) =
        use cmd = this.BuildCommand(query.ToKataQuery())
        use reader = cmd.ExecuteReader() :?> 'Reader
        let read = getReaders reader
        seq [| 
            while reader.Read() do
                read() 
        |] 

    member this.ReadOne<'Entity, 'Reader when 'Reader :> DbDataReader> (getReaders: 'Reader -> (unit -> 'Entity)) (query: SelectQuery<'Entity>) =
        this.Read getReaders query |> Seq.tryHead

    member this.ReadAsync<'Entity, 'Reader when 'Reader :> DbDataReader> (getReaders: 'Reader -> (unit -> 'Entity)) (query: SelectQuery<'Entity>) = 
        this.ReadAsyncWithCancellation getReaders CancellationToken.None query
    
    member this.ReadAsyncWithCancellation<'Entity, 'Reader when 'Reader :> DbDataReader>
        (getReaders: 'Reader -> (unit -> 'Entity)) (cancellationToken: CancellationToken) (query: SelectQuery<'Entity>) = 
        async {
            use cmd = this.BuildCommand(query.ToKataQuery())
            use! reader = cmd.ExecuteReaderAsync(cancellationToken) |> Async.AwaitTask
            let read = getReaders (reader :?> 'Reader)
            return
                seq [| 
                    while reader.Read() do
                        read() 
                |]
        }
        |> Async.StartImmediateAsTask

    member this.ReadOneAsync<'Entity, 'Reader when 'Reader :> DbDataReader> (getReaders: 'Reader -> (unit -> 'Entity)) (query: SelectQuery<'Entity>) = 
        this.ReadOneAsyncWithCancellation getReaders CancellationToken.None query

    member this.ReadOneAsyncWithCancellation<'Entity, 'Reader when 'Reader :> DbDataReader>
        (getReaders: 'Reader -> (unit -> 'Entity)) (cancellationToken: CancellationToken) (query: SelectQuery<'Entity>) = 
        async {
            let! entities = this.ReadAsyncWithCancellation getReaders cancellationToken query |> Async.AwaitTask
            return entities |> Seq.tryHead
        }
        |> Async.StartImmediateAsTask

    member this.Insert<'T, 'InsertReturn when 'InsertReturn : struct> (query: InsertQuery<'T, 'InsertReturn>) = 
        use cmd = this.BuildCommand(query.ToKataQuery())
        // Did the user select an identity field?
        match query.Spec.IdentityField with
        | Some identityField -> 
            if compiler :? SqlKata.Compilers.PostgresCompiler then 
                // Replace PostgreSQL identity query
                cmd.CommandText <- cmd.CommandText.Replace(";SELECT lastval() AS id", $" RETURNING {identityField};")

            let identity = cmd.ExecuteScalar()
            // 'InsertReturn type set via `getId` in the builder
            System.Convert.ChangeType(identity, typeof<'InsertReturn>) :?> 'InsertReturn
        
        | None ->
            let results = cmd.ExecuteNonQuery()
            // 'InsertReturn is `int` here -- NOTE: must include `'InsertReturn : struct` constraint
            System.Convert.ChangeType(results, typeof<'InsertReturn>) :?> 'InsertReturn

    member this.InsertAsync<'T, 'InsertReturn when 'InsertReturn : struct> (query: InsertQuery<'T, 'InsertReturn>) = 
        this.InsertAsync(CancellationToken.None, query)
    
    member this.InsertAsync<'T, 'InsertReturn when 'InsertReturn : struct> (cancellationToken: CancellationToken, query: InsertQuery<'T, 'InsertReturn>) = 
        async {
            use cmd = this.BuildCommand(query.ToKataQuery())
            // Did the user select an identity field?
            match query.Spec.IdentityField with
            | Some identityField -> 
                if compiler :? SqlKata.Compilers.PostgresCompiler then 
                    // Replace PostgreSQL identity query
                    cmd.CommandText <- cmd.CommandText.Replace(";SELECT lastval() AS id", $" RETURNING {identityField};")

                let! identity = cmd.ExecuteScalarAsync(cancellationToken) |> Async.AwaitTask
                // 'InsertReturn type set via `getId` in the builder
                return System.Convert.ChangeType(identity, typeof<'InsertReturn>) :?> 'InsertReturn
        
            | None ->
                let! results = cmd.ExecuteNonQueryAsync(cancellationToken) |> Async.AwaitTask
                // 'InsertReturn is `int` here -- NOTE: must include `'InsertReturn : struct` constraint
                return System.Convert.ChangeType(results, typeof<'InsertReturn>) :?> 'InsertReturn
        }
        |> Async.StartImmediateAsTask
    
    member this.Update (query: UpdateQuery<'T>) = 
        use cmd = this.BuildCommand(query.ToKataQuery())
        cmd.ExecuteNonQuery()

    member this.UpdateAsync (query: UpdateQuery<'T>) = 
        this.UpdateAsync(CancellationToken.None, query)
    
    member this.UpdateAsync (cancellationToken: CancellationToken, query: UpdateQuery<'T>) = 
        use cmd = this.BuildCommand(query.ToKataQuery())
        cmd.ExecuteNonQueryAsync(cancellationToken)

    member this.Delete (query: DeleteQuery<'T>) = 
        use cmd = this.BuildCommand(query.ToKataQuery())
        cmd.ExecuteNonQuery()

    member this.DeleteAsync (query: DeleteQuery<'T>) = 
        this.DeleteAsync(CancellationToken.None, query)

    member this.DeleteAsync (cancellationToken: CancellationToken, query: DeleteQuery<'T>) = 
        use cmd = this.BuildCommand(query.ToKataQuery())
        cmd.ExecuteNonQueryAsync(cancellationToken)

    member this.Count (query: SelectQuery<int>) = 
        use cmd = this.BuildCommand(query.ToKataQuery())
        match cmd.ExecuteScalar() with
        | :? int64 as count -> System.Convert.ToInt32 count
        | _  as count -> count :?> int

    member this.CountAsync (query: SelectQuery<int>) = 
        this.CountAsync(CancellationToken.None, query)

    member this.CountAsync (cancellationToken: CancellationToken, query: SelectQuery<int>) = 
        async {
            use cmd = this.BuildCommand(query.ToKataQuery())
            let! count = cmd.ExecuteScalarAsync(cancellationToken) |> Async.AwaitTask
            return 
                match count with
                | :? int64 as value -> System.Convert.ToInt32 value
                | _ -> count :?> int
        }
        |> Async.StartImmediateAsTask