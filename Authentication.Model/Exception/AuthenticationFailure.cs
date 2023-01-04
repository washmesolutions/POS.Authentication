using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Authentication.Model.Exception
{
    [Serializable]
    public class AuthenticationFailure : System.Exception
    {
        private string _redirectLocation;
        public string SamlRequest { get; set; }
        public int StatusCode { get; set; }

        public AuthenticationFailure(string redirectLocation)
            : base(string.Empty)
        {
            this._redirectLocation = redirectLocation;
            this.StatusCode = 403;
        }

        public string RedirectLocation
        {
            get
            {
                return _redirectLocation;
            }
        }

        public AuthenticationFailure(string redirectLocation, string message)
            : base(message)
        {
            this.StatusCode = 403;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (info != null && !string.IsNullOrEmpty(this._redirectLocation))
            {
                info.AddValue("RedirectLocation", this._redirectLocation);
            }
            if (info != null && !string.IsNullOrEmpty(this.SamlRequest))
            {
                info.AddValue("SAMLRequest", this.SamlRequest);
            }
        }
    }
}
