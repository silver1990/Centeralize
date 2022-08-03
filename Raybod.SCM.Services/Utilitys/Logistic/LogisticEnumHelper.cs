using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.Services.Utilitys.Logistic
{
    public static class LogisticEnumHelper
    {
        //public static List<AddressType> ReturnOriginAndDestinationAddressStatus(PackStatus packStatus, List<AddressType> addressesType)
        //{
        //    switch (packStatus)
        //    {
        //        case PackStatus.AcceptQC:
        //            addressesType = new List<AddressType> { AddressType.OriginPort };
        //            break;

        //        case PackStatus.ClearanceOfOriginPort:
        //            addressesType = new List<AddressType> { AddressType.OriginPort, AddressType.DestinationPort };
        //            break;

        //        case PackStatus.ClearanceOfDestinationPort:
        //            addressesType = new List<AddressType> { AddressType.DestinationPort };
        //            break;
        //    }

        //    return addressesType;
        //}

        //public static AddressType ReturnOriginStatusByTransportationStatus(TransportationStatus status)
        //{
        //    switch (status)
        //    {
        //        case TransportationStatus.OriginPort:
        //            return AddressType.SupplierLocation;
        //        case TransportationStatus.DestinationPort:
        //            return AddressType.OriginPort;
        //        case TransportationStatus.CompanyLocation:
        //            return AddressType.DestinationPort;
        //        case TransportationStatus.FromSupplierToCompanyLocation:
        //            return AddressType.SupplierLocation;
        //        default:
        //            return AddressType.SupplierLocation;
        //    }
        //}

        //public static AddressType ReturnDestinationStatusByTransportationStatus(TransportationStatus status)
        //{
        //    switch (status)
        //    {
        //        case TransportationStatus.OriginPort:
        //            return AddressType.OriginPort;
        //        case TransportationStatus.DestinationPort:
        //            return AddressType.DestinationPort;
        //        case TransportationStatus.CompanyLocation:
        //            return AddressType.CompanyLocation;
        //        case TransportationStatus.FromSupplierToCompanyLocation:
        //            return AddressType.CompanyLocation;
        //        default:
        //            return AddressType.CompanyLocation;
        //    }
        //}

        //public static PackStatus ReturnPackStatusByPackStatusAndcontractType(PContractType contractType, PackStatus packStatus)
        //{
        //    if (contractType == PContractType.Foreign)
        //    {
        //        switch (packStatus)
        //        {
        //            case PackStatus.AcceptQC:
        //                return PackStatus.T1Pending;

        //            case PackStatus.T1Pending:
        //                return PackStatus.OriginPort;

        //            case PackStatus.OriginPort:
        //                return PackStatus.ClearanceOfOriginPort;

        //            case PackStatus.ClearanceOfOriginPort:
        //                return PackStatus.TransportationToDestinationPort;

        //            case PackStatus.TransportationToDestinationPort:
        //                return PackStatus.DestinationPort;

        //            case PackStatus.DestinationPort:
        //                return PackStatus.ClearanceOfDestinationPort;

        //            case PackStatus.ClearanceOfDestinationPort:
        //                return PackStatus.TransportationToCompanyLocation;

        //            case PackStatus.TransportationToCompanyLocation:
        //                return PackStatus.CompanyLocation;

        //            case PackStatus.CompanyLocation:
        //                return PackStatus.CompanyLocation;

        //            default:
        //                return PackStatus.CompanyLocation;
        //        }
        //    }
        //    else
        //    {
        //        switch (packStatus)
        //        {
        //            case PackStatus.AcceptQC:
        //                return PackStatus.TransportationToCompanyLocation;

        //            case PackStatus.CompanyLocation:
        //                return PackStatus.CompanyLocation;

        //            default:
        //                return PackStatus.CompanyLocation;
        //        }
        //    }
        //}

        //public static bool IsPackStatusAcceptableForNewTransportation(PackStatus status)
        //{
        //    var acceptableList = new List<PackStatus>
        //    {
        //        PackStatus.AcceptQC,
        //        PackStatus.ClearanceOfOriginPort,
        //        PackStatus.ClearanceOfDestinationPort
        //    };
        //    return acceptableList.Contains(status);
        //}

        //public static TransportationStatus ReturnTransportationStatusOfThisPackByPackStatus(PContractType contractType, PackStatus packStatus)
        //{
        //    if (contractType == PContractType.Internal)
        //        return TransportationStatus.FromSupplierToCompanyLocation;

        //    switch (packStatus)
        //    {
        //        case PackStatus.AcceptQC:
        //            return TransportationStatus.OriginPort;

        //        case PackStatus.ClearanceOfOriginPort:
        //            return TransportationStatus.DestinationPort;

        //        case PackStatus.ClearanceOfDestinationPort:
        //            return TransportationStatus.CompanyLocation;

        //        default:
        //            return TransportationStatus.None;

        //    }
        //}

        //public static bool IsPackStatusAcceptableForNewClearancePort(PackStatus status)
        //{
        //    var acceptableList = new List<PackStatus>
        //    {
        //        PackStatus.AcceptQC,
                
        //    };
        //    return acceptableList.Contains(status);
        //}

        //public static TransportationStatus ReturnTransportationStatus(PackStatus status)
        //{
        //    switch (status)
        //    {
              
        //        default:
        //            return TransportationStatus.DestinationPort;

        //    }
        //}

        //public static List<PackStatus> GetWaitingPacksStatusForTransportation()
        //{
        //    return new List<PackStatus> {
        //        PackStatus.AcceptQC,
        //    };
        //}

        //public static List<PackStatus> GetWaitingPacksStatusForClearancePort()
        //{
        //    return new List<PackStatus> {               
        //        PackStatus.AcceptQC
        //    };
        //}

        //public static POStatus GetPoStatusOFThisPackStatus(PackStatus packStatus)
        //{
        //    return POStatus.TransportationToCompanyLocation;
        //}
    
    }
}
