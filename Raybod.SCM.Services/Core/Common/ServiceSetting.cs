using System;

namespace Raybod.SCM.Services.Core.Common
{
    public static class ServiceSetting
    {
        public struct UploadImagesPath
        {
            public const string UserSmall = "/Files/UploadImages/user/small/";
            public const string UserLarge = "/Files/UploadImages/user/large/";
            public const string ProductSmall = "/Files/UploadImages/Product/small/";
            public const string ProductLarge = "/Files/UploadImages/Product/large/";
            public const string LogoLarge = "/Files/UploadImages/logo/large/";
            public const string LogoSmall = "/Files/UploadImages/logo/small/";
            public const string Small = "/Files/UploadImages/small/";
            public const string Medium = "/Files/UploadImages/medium/";
            public const string Large = "/Files/UploadImages/large/";
            public const string Fhd = "/Files/UploadImages/fhd/";
            public const string Temp = "/Files/UploadImages/temp/";
        }

        public struct UploadFilePath
        {
            public const string Payment = "/Files/UploadDocument/Payment/";
            public const string Invoice = "/Files/UploadDocument/Invoice/";
            public const string ContractDocument = "/Files/UploadDocument/ContractDocument/";
            public const string TechnicalDocument = "/Files/UploadDocument/TechnicalDocument/";
            public const string EngineeringDocument = "/Files/UploadDocument/EngineeringDocument/";
            public const string PR = "/Files/UploadDocument/PR/";
            public const string RFP = "/Files/UploadDocument/RFP/";
            public const string RFPComment = "/Files/UploadDocument/RFPComment/";
            public const string POComment = "/Files/UploadDocument/PO/POComment/";
            public const string RFPSupplier = "/Files/UploadDocument/RFPSupplier/";

            public const string PrContract = "/Files/UploadDocument/PrContract/";
            public const string PO = "/Files/UploadDocument/PO/";
            public const string POInspection = "/Files/UploadDocument/PO/POInspection/";
            public const string POManufactureDocument = "/Files/UploadDocument/PO/POManufactureDocument/";
            public const string Temp = "/Files/UploadDocument/temp/";
        }

        public static string UploadFilePathContract(string contractCode)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Contract/";
        }
        public static string FileDriveRootPath(string contractCode)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/FileDrive/";
        }
        public static string PrivateFileDriveRootPath(string contractCode,string userName)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/FileDrive/{userName}/";
        }
        public static string FileDriveTrashPath(string contractCode)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/FileDrive/Trash/";
        }
        public static string UploadFilePathDocument(string contractCode, long docId, long revisionId)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Document/{docId}/Reviosin/{revisionId}/";
        }
        public static string UploadFilePathOperation(string contractCode, Guid operationId)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Operation/{operationId}/";
        }
        public static string UploadFilePathTransmittal(string contractCode)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Document/Transmittal/";
        }
        public static string UploadFilePathRevisionCommunication(string contractCode, long docId, long revisionId)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Document/{docId}/Reviosin/{revisionId}/Communication/";
        }

        public static string UploadFilePathDocumentTransMittal(string contractCode)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Document/Transmittal/";
        }


        public static string UploadFilePathRevisionComment(string contractCode, long docId, long revisionId)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Document/{docId}/Reviosin/{revisionId}/Comment/";
        }
        public static string UploadFilePathFileDriveComment(string contractCode)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/FileDrive/Comment/";
        }
        public static string UploadFilePathOperationComment(string contractCode, Guid operationId)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Operation/{operationId}/Comment/";
        }

        public static string UploadFilePathPR(string contractCode, long prId)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/PR/{prId}/";
        }
        public static string UploadFilePathPRConfirm(string contractCode, long mrpId)
        {
            return $"/Files/UploadDocument/PR/";
        }

        public static string UploadFilePathRFP(string contractCode, long rfpId)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/RFP/{rfpId}/";
        }
        public static string UploadFilePathRFPComment(string contractCode, long rfpId)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/RFP/{rfpId}/Comment/";
        }

        public static string UploadFilePathPrContract(string contractCode, long prContractId)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/PrContract/{prContractId}/";
        }

        public static string UploadFilePathPO(string contractCode, long poId)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/PO/{poId}/";
        }

        public static string UploadFilePathPayment(string contractCode)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Payment/";
        }

        public static string UploadFilePathInvoice(string contractCode)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Invoice/";
        }

        public static string UploadFilePathFinancial(string contractCode)
        {
            return $"/Files/UploadDocument/{contractCode.ToLower()}/Financial/";
        }

        public const int DefaultNotSaveErrorCode = -1;
        public const string OrderByFullName = "FullName:Asc";
        public const string OrderByDate = "Date:Asc";
        public const string DescendingOrderByDate = "Date:Desc";
        public const string DescendingOrderById = "Id:Desc";
        public const int SmallDefaultPageSize = 10;
        public const int MediumDefaultPageSize = 50;
        public const int LargeDefaultPageSize = 120;
        public const int ExtraLargeDefaultPageSize = 300;
        public const int MaxDefaultPageSize = int.MaxValue;
        public const string HostUrl = "http://10.10.1.219:5003";
        //https://localhost:44382
        //public const string KeyAESEncription = "6ad217d5d3a62fc3276b7f4e4805fcc6";

        public enum FileSection
        {
            Document = 1,
            RFP = 2,
            RFPSupplier = 3,
            PRContract = 4,
            PO = 5,
            ContractDocument = 6,
            PR = 7,
            RFPComment = 8,
            Payment = 9,
            Invoice = 10,
            POComment = 11,
            PRWorkFlow=12,
            PoIncpection=13,
            ManufactureDocument=14

        }

    }
}