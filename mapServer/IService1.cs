using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data;

namespace BleLog
{

    [ServiceContract]
    public interface IBleLog
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/GetLogdata", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType(typeof(List<Logdata>))]
        IEnumerable<Logdata> GetLogdata(DataRequest query);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/GetSensorNames", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType(typeof(List<Sensor>))]
        IEnumerable<Sensor> GetSensorNames(int userID);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/Login", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Login Login(Login login);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/Signup", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string Signup(Login login);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SaveLogdata", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        UploadResult SaveLogdata(IEnumerable<Logdata> logdata);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SaveSensor", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string SaveSensor(Sensor sensor);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SaveLocation", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string SaveLocation(location loc);
    }
}