using Raybod.SCM.Utility.EnumType;

namespace Raybod.SCM.Utility.Security
{
    public class AuthenticationToken
    {
        private readonly AES.RijndaelCrypt _coder;
        public int Id { get; set; }

        public string UserName { get; set; }

        public string ReagentCode { get; set; }

        public UserType UserType { get; set; }

        /// <summary>
        /// آیا این کاربر مهمان است؟
        /// </summary>
        public bool IsGuest { get; set; }

        public bool IsActive { get; set; }

        public bool  IsDriver{ get; set; }

        public AuthenticationToken(int id, string userName, string reagentCode, UserType userType, bool isActive,bool isDriver=false)
        {
            Id = id;
            UserName = userName;
            ReagentCode = reagentCode;
            UserType = userType;
            _coder = new AES.RijndaelCrypt("Sim3McZa!oqpaPp");
            IsActive = isActive;
            IsDriver = isDriver;
        }

        public AuthenticationToken()
        {
            _coder = new AES.RijndaelCrypt("Sim3McZa!oqpaPp");
        }

        public string SetAuthenticationTiket()
        {
            return _coder.Encrypt(Newtonsoft.Json.JsonConvert.SerializeObject(new AuthenticationToken(Id, UserName, ReagentCode, UserType, IsActive,IsDriver)));
        }

        public void GetAuthenticationTiket(string value)
        {
            var result = _coder.Decrypt(value);
            var deserialazedTiket = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticationToken>(result);
            Id = deserialazedTiket.Id;
            UserName = deserialazedTiket.UserName;
            ReagentCode = deserialazedTiket.ReagentCode;
            IsActive = deserialazedTiket.IsActive;
            UserType = deserialazedTiket.UserType;
            IsDriver = deserialazedTiket.IsDriver;
        }

    }
}