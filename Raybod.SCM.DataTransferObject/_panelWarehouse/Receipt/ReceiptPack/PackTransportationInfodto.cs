namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class PackTransportationInfoDto
    {
        public long TransportationId { get; set; }
       
        public string TransportationNumber { get; set; }

        public string DelivererName { get; set; }

        public string DelivererVehicle { get; set; }

        public long? DeliverDate { get; set; }

        public string DeliverPhoneNumber { get; set; }
    }
}
