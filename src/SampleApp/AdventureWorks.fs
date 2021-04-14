//------------------------------------------------------------------------------
//        This code was generated by myriad.
//        Changes to this file will be lost when the code is regenerated.
//------------------------------------------------------------------------------
namespace AdventureWorks

module dbo =
    type BuildVersion =
        { SystemInformationID: byte
          ``Database Version``: string
          VersionDate: System.DateTime
          ModifiedDate: System.DateTime }

    type ErrorLog =
        { ErrorLogID: int
          ErrorTime: System.DateTime
          ErrorNumber: int
          ErrorSeverity: Option<int>
          ErrorState: Option<int>
          ErrorProcedure: Option<string>
          ErrorLine: Option<int>
          ErrorMessage: string }

module SalesLT =
    type Address =
        { AddressID: int
          AddressLine1: string
          AddressLine2: Option<string>
          City: string
          PostalCode: string
          rowguid: System.Guid
          ModifiedDate: System.DateTime }

    type Customer =
        { CustomerID: int
          Title: Option<string>
          Suffix: Option<string>
          CompanyName: Option<string>
          SalesPerson: Option<string>
          EmailAddress: Option<string>
          PasswordHash: string
          PasswordSalt: string
          rowguid: System.Guid
          ModifiedDate: System.DateTime }

    type CustomerAddress =
        { CustomerID: int
          AddressID: int
          rowguid: System.Guid
          ModifiedDate: System.DateTime }

    type Product =
        { ProductID: int
          ProductNumber: string
          Color: Option<string>
          StandardCost: decimal
          ListPrice: decimal
          Size: Option<string>
          Weight: Option<decimal>
          ProductCategoryID: Option<int>
          ProductModelID: Option<int>
          SellStartDate: System.DateTime
          SellEndDate: Option<System.DateTime>
          DiscontinuedDate: Option<System.DateTime>
          ThumbNailPhoto: Option<byte []>
          ThumbnailPhotoFileName: Option<string>
          rowguid: System.Guid
          ModifiedDate: System.DateTime }

    type ProductCategory =
        { ProductCategoryID: int
          ParentProductCategoryID: Option<int>
          rowguid: System.Guid
          ModifiedDate: System.DateTime }

    type ProductDescription =
        { ProductDescriptionID: int
          Description: string
          rowguid: System.Guid
          ModifiedDate: System.DateTime }

    type ProductModel =
        { ProductModelID: int
          CatalogDescription: Option<System.Xml.Linq.XElement>
          rowguid: System.Guid
          ModifiedDate: System.DateTime }

    type ProductModelProductDescription =
        { ProductModelID: int
          ProductDescriptionID: int
          Culture: string
          rowguid: System.Guid
          ModifiedDate: System.DateTime }

    type SalesOrderDetail =
        { SalesOrderID: int
          SalesOrderDetailID: int
          OrderQty: System.Int16
          ProductID: int
          UnitPrice: decimal
          UnitPriceDiscount: decimal
          LineTotal: decimal
          rowguid: System.Guid
          ModifiedDate: System.DateTime }

    type SalesOrderHeader =
        { SalesOrderID: int
          RevisionNumber: byte
          OrderDate: System.DateTime
          DueDate: System.DateTime
          ShipDate: Option<System.DateTime>
          Status: byte
          SalesOrderNumber: obj
          CustomerID: int
          ShipToAddressID: Option<int>
          BillToAddressID: Option<int>
          ShipMethod: string
          CreditCardApprovalCode: Option<string>
          SubTotal: decimal
          TaxAmt: decimal
          Freight: decimal
          TotalDue: obj
          Comment: Option<string>
          rowguid: System.Guid
          ModifiedDate: System.DateTime }

    type TestTable = { Id: int; Description: string }
    type vGetAllCategories = { ProductCategoryID: int }
    type vProductAndDescription =
        { ProductID: int
          Culture: string
          Description: string
          Three: int }

    type vProductModelCatalogDescription =
        { ProductModelID: int
          Summary: Option<System.Xml.Linq.XElement>
          Manufacturer: Option<System.Xml.Linq.XElement>
          Copyright: Option<System.Xml.Linq.XElement>
          ProductURL: Option<System.Xml.Linq.XElement>
          WarrantyPeriod: Option<System.Xml.Linq.XElement>
          WarrantyDescription: Option<System.Xml.Linq.XElement>
          NoOfYears: Option<System.Xml.Linq.XElement>
          MaintenanceDescription: Option<System.Xml.Linq.XElement>
          Wheel: Option<System.Xml.Linq.XElement>
          Saddle: Option<System.Xml.Linq.XElement>
          Pedal: Option<System.Xml.Linq.XElement>
          BikeFrame: Option<System.Xml.Linq.XElement>
          Crankset: Option<System.Xml.Linq.XElement>
          PictureAngle: Option<System.Xml.Linq.XElement>
          PictureSize: Option<System.Xml.Linq.XElement>
          ProductPhotoID: Option<System.Xml.Linq.XElement>
          Material: Option<System.Xml.Linq.XElement>
          Color: Option<System.Xml.Linq.XElement>
          ProductLine: Option<System.Xml.Linq.XElement>
          Style: Option<System.Xml.Linq.XElement>
          RiderExperience: Option<System.Xml.Linq.XElement>
          rowguid: System.Guid
          ModifiedDate: System.DateTime }
