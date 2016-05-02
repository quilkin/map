using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace BleLog
{



    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class BleLog : IBleLog, IDisposable
    {

        SqlConnection ttConnection;

        DataTable dataLogs;
        List<Logdata> logdata;
        List<Sensor> sensors;

        DataTable dataLogins;
        //int currentID;


        ////string connection = "server=www.timetrials.org.uk;database=chrisfgma5886com5778_timetrial;User ID=quilkin;Password=Iw3692eh";
        //string connection = ConfigurationManager.ConnectionStrings["BleLog"].ConnectionString;
        //string smtpserver = "mail.quilkin.co.uk";
        //string smtpUserName = "admin@quilkin.co.uk";
        //string smtpPassword = "icespy1643";

        string connection = Connections.connection;
        string smtpserver = Connections.smtpserver;
        string smtpUserName = Connections.smtpUserName;
        string smtpPassword = Connections.smtpPassword;

        public static string TimeString(DateTime time)
        {
            if (time == DateTime.MinValue)
                return System.DBNull.Value.ToString();
            //return time.ToString("G", DateTimeFormatInfo.InvariantInfo);
            return string.Format("{0}{1}{2} {3}:{4}:{5}",
                time.Year, time.Month.ToString("00"), time.Day.ToString("00"),
                time.Hour.ToString("00"), time.Minute.ToString("00"), time.Second.ToString("00"));
        }

        public BleLog()
        {
        }
        public void Dispose()
        {
            if (dataLogs != null)
                dataLogs.Dispose();
            if (dataLogins != null)
                dataLogins.Dispose();
            if (ttConnection != null)
                ttConnection.Dispose();
        }

        private string getIP()
        {
            OperationContext oOperationContext = OperationContext.Current;
            MessageProperties oMessageProperties = oOperationContext.IncomingMessageProperties;
            RemoteEndpointMessageProperty oRemoteEndpointMessageProperty = (RemoteEndpointMessageProperty)oMessageProperties[RemoteEndpointMessageProperty.Name];

            string szAddress = oRemoteEndpointMessageProperty.Address;
            int nPort = oRemoteEndpointMessageProperty.Port;
            return szAddress;
        }

        public IEnumerable<Sensor> GetSensorNames(int userID)
        {

            LogEntry log = new LogEntry(getIP(), "GetSensorNames", userID.ToString());

            sensors = new List<Sensor>();

            if (ttConnection == null)
            {
                try
                {
                    ttConnection = new SqlConnection(connection);
                    ttConnection.Open();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    return null;
                }
            }

            // get sensors associated to that user
            string query = string.Format("SELECT * FROM sensors where sensors.owner = {0}", userID);

            using (SqlDataAdapter riderAdapter = new SqlDataAdapter(query, ttConnection))
            {
                dataLogs = new DataTable();
                riderAdapter.Fill(dataLogs);
                // ToDo: not efficient to convert table to  List<> in order to provide the data
                int length = dataLogs.Rows.Count;
                for (int row = 0; row < length; row++)
                {
                    string name = "", serial = "", descrip = "";
                    int id, period = 60, alarmlow = 0, alarmhigh = 0;
                    try
                    {
                        DataRow dr = dataLogs.Rows[row];
                        id = (int)dr["id"];
                        try { name = (string)dr["name"]; } catch { }
                        try { serial = (string)dr["serial"]; } catch { }
                        try { descrip = (string)dr["descrip"]; } catch { }
                        try { alarmlow = (int)dr["alarmlow"]; } catch { }
                        try { alarmhigh = (int)dr["alarmhigh"]; } catch { }
                        try { period = (int)dr["period"]; } catch { }

                        sensors.Add(new Sensor(id, serial, name, descrip, alarmlow, alarmhigh, period, userID));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                        log.Error = ex.Message;
                    }
                }
            }
            log.Result = sensors.Count.ToString() + " sensors";
            log.Save(ttConnection);
            ttConnection.Close();
            return sensors;
        }

        public string SaveSensor(Sensor sensor)
        {
            LogEntry log = new LogEntry(getIP(), "SaveSensor", sensor.Serial + " " + sensor.Name);

            int successRows = 0;
            string result = "";
            try
            {
                ttConnection = new SqlConnection(connection);
                ttConnection.Open();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return ex.Message;

            }
            try
            {

                // check sensor isn't already there***************

                string query = string.Format("SELECT serial FROM sensors where sensors.serial = '{0}'", sensor.Serial);
                bool exists = true;
                string now = TimeString(DateTime.Now);
                using (SqlDataAdapter sensorAdapter = new SqlDataAdapter(query, ttConnection))
                {
                    dataLogins = new DataTable();
                    sensorAdapter.Fill(dataLogins);

                    if (dataLogins.Rows.Count == 0)
                    {
                        exists = false;
                    }
                }
                if (exists)
                    query = string.Format("update sensors set name='{0}',descrip = '{1}',alarmlow ='{2}',alarmhigh= '{3}',period='{4}',timeadded='{5}' where sensors.serial = '{6}'",
                        sensor.Name, sensor.Description, sensor.AlarmLow, sensor.AlarmHigh, sensor.Period, now, sensor.Serial);
                else
                    query = string.Format("insert into sensors (serial,name,owner,period,descrip,alarmlow,alarmhigh,timeadded) values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                        sensor.Serial, sensor.Name, sensor.Owner, sensor.Period, sensor.Description, sensor.AlarmLow, sensor.AlarmHigh, now);



                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, ttConnection))
                {
                    successRows = command.ExecuteNonQuery();
                }
                if (successRows == 1)
                    result = string.Format("Sensor {0} saved OK", sensor.Name);
                else
                    result = string.Format("Database error: sensor {0} not saved: {1} rows changed", sensor.Serial, successRows);

            }

            catch (Exception ex)
            {

                Trace.WriteLine(ex.Message);
                log.Error = ex.Message;
                //return ex.Message;
            }


            finally
            {
                log.Result = result;
                log.Save(ttConnection);
                ttConnection.Close();
            }
            return result;

        }


        /// <summary>
        /// Get all saved data for set of  given devices between specified times.
        /// Times are passed as 'smalldatetime' i.e. number of minutes since 01.01.1970
        /// </summary>
        /// <param name="id">device serial number</param>
        /// <param name="from">start of data reqd</param>
        /// <param name="to">end of data reqd</param>
        /// <returns></returns>
        public IEnumerable<Logdata> GetLogdata(DataRequest req)
        {
            LogEntry log = new LogEntry(getIP(), "GetLogdata", req.ToString());


            logdata = new List<Logdata>();
            bool closeneeded = false;
            //int thisID = 0;

            if (ttConnection == null)
            {
                try
                {
                    ttConnection = new SqlConnection(connection);
                    ttConnection.Open();
                    closeneeded = true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);

                }
            }

            string idList = string.Empty;
            foreach (int id in req.IDlist)
            {
                idList += id.ToString();
                idList += ',';
            }
            //char[] comma = { };
            idList = idList.TrimEnd(',');


            string query = string.Format("SELECT logdata.id, logdata.time, logdata.value FROM logdata  WHERE logdata.id in ( {0} ) and logdata.time >= '{1}' and logdata.time <= '{2}' ORDER BY logdata.time,logdata.id",
                idList, req.From, req.To);
            using (SqlDataAdapter riderAdapter = new SqlDataAdapter(query, ttConnection))
            {
                dataLogs = new DataTable();
                riderAdapter.Fill(dataLogs);
                // ToDo: not efficient to convert table to  List<> in order to provide the data
                int length = dataLogs.Rows.Count;
                ArrayList IDs = new ArrayList();
                List<float> vals = new List<float>();
                // initalise vals with 'missing data'
                foreach (int i in req.IDlist)
                    vals.Add(-273);
                int time = 0, oldTime = 0;
                DataRow dr;

                // Need to assemble data into values with the same timestamps.
                // If data is missing for some IDs at a certain time, need to insert 'missing data' values

                // First, need to run through list to find all the IDs it may contain - some IDs may not be present in the first few records,
                //    and we need to have the finished list in ID order.
                for (int row = 0; row < length; row++)
                {
                    try
                    {
                        dr = dataLogs.Rows[row];
                        int id = (int)dr["id"];

                        int idIndex = IDs.IndexOf(id);
                        if (idIndex < 0)
                        {
                            // haven't seen one of these before
                            IDs.Add(id);
                        }

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                        log.Error = ex.Message;
                    }
                }
                if (IDs.Count < req.IDlist.Count)
                {
                    // one sensor asked for has no data at all
                    foreach (int id in req.IDlist)
                    {
                        if (IDs.Contains(id) == false)
                        {
                            IDs.Add(id);
                        }
                    }
                }
                IDs.Sort();
                // now actually sort the data
                for (int row = 0; row < length; row++)
                {
                    try
                    {
                        dr = dataLogs.Rows[row];
                        time = (int)dr["time"];
                        int id = (int)dr["id"];
                        //if (oldTime == 0) oldTime = time;

                        int idIndex = IDs.IndexOf(id);
                        float val = (float)dr["value"];
                        vals[idIndex] = val;
                        if (time != oldTime)
                        {
                            // this will be a new record
                            logdata.Add(new Logdata(time, vals));
                            //vals.Clear();
                            vals = new List<float>();
                            // initalise new vals with 'missing data'
                            foreach (int i in req.IDlist)
                                vals.Add(-273);
                            oldTime = time;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                        log.Error = ex.Message;
                    }
                }
            }
            log.Result = "logdata size: " + logdata.Count;
            log.Save(ttConnection);

            if (closeneeded)
                ttConnection.Close();
            return logdata;
        }

        /// <summary>
        /// Save a chunk of data (for a single device)
        /// </summary>
        /// <param name="newdata"></param>
        /// <returns></returns>
        public UploadResult SaveLogdata(IEnumerable<Logdata> newdata)
        {
            LogEntry log = new LogEntry(getIP(), "SaveLogdata", newdata.Count().ToString() + " records");
            log.Error = "Starting SaveLogdata";


            // find start and end times of new data
            int firstnewdata = int.MaxValue;
            int lastnewdata = int.MinValue;
            string serial = string.Empty;
            int thisID = 0;

            try
            {
                ttConnection = new SqlConnection(connection);
                ttConnection.Open();

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);

            }
            log.Save(ttConnection);
            try
            {
                foreach (Logdata data in newdata)
                {
                    if (data.T > lastnewdata)
                        lastnewdata = data.T;
                    if (data.T < firstnewdata)
                        firstnewdata = data.T;
                    if (serial == string.Empty) serial = data.S;
                    else if (data.S != null && data.S.Length > 0 && serial != data.S)
                    {
                        string err = "Cannot save data, contains values from more than one device";
                        Trace.WriteLine(err);
                        log.Error = err;
                        log.Save(ttConnection);
                        ttConnection.Close();
                        return new UploadResult(0, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                log.Error = ex.Message;
                log.Save(ttConnection);
                ttConnection.Close();
                return new UploadResult(0, 0);
            }
            if (serial.Length > 20)
                serial = serial.Substring(0, 18);
            log.Error = "SaveLogdata, serial = " + serial;
            log.Save(ttConnection);

            // find sensor ID from serial number
            string query = string.Format("SELECT sensors.id FROM sensors  WHERE sensors.serial = '{0}' ", serial);
            using (SqlDataAdapter idAdapter = new SqlDataAdapter(query, ttConnection))
            {
                dataLogs = new DataTable();
                idAdapter.Fill(dataLogs);
                // ToDo: not efficient to convert table to  List<> in order to provide the data
                if (dataLogs.Rows.Count > 0)
                {

                    try
                    {
                        DataRow dr = dataLogs.Rows[0];
                        thisID = (int)dr["id"];
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                        log.Error = ex.Message;
                        log.Save(ttConnection);
                        ttConnection.Close();
                        return new UploadResult(0, 0);
                    }
                }
                log.Error = "SaveLogdata, rows = " + dataLogs.Rows.Count;
                log.Save(ttConnection);
            }



            List<int> ids = new List<int>();
            int firstolddata = int.MaxValue;
            int lastolddata = int.MinValue;

            if (thisID > 0)
            {
                ids.Add(thisID);
                // now get any existing data between these times. Just one ID to put into the array of requests
                if (GetLogdata(new DataRequest(ids, firstnewdata, lastnewdata)) != null)
                {
                    foreach (Logdata data in logdata)
                    {
                        if (data.T >= lastolddata)
                            lastolddata = data.T;
                        if (data.T <= firstolddata)
                            firstolddata = data.T;
                    }
                }
            }
            else
            {
                // ID not found, must be a new sensor
                try
                {
                    query = string.Format("insert into sensors (serial,name,owner,timeadded) values ('{0}','{1}','{2}','{3}')", serial, "no name", 0, TimeString(DateTime.Now));
                    using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, ttConnection))
                    {
                        command.ExecuteNonQuery();

                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    log.Error = ex.Message;
                    log.Save(ttConnection);
                    ttConnection.Close();
                    return new UploadResult(0, 0);
                }

            }

            log.Error = "SaveLogdata 3";
            log.Save(ttConnection);

            int saved = 0, notsaved = 0;
            UploadResult result = null;
            try
            {
                foreach (Logdata data in newdata)
                {
                    if (data.T >= firstolddata && data.T <= lastolddata)
                    {
                        // data already stored for this time (unless gaps have somehow been introduced??)
                        ++notsaved;
                        continue;
                    }

                    query = string.Format("insert into logdata (id, time, value) values ('{0}','{1}','{2}')\n\r",
                        thisID, data.T, data.V[0]);

                    using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, ttConnection))
                    {
                        command.ExecuteNonQuery();
                        ++saved;
                    }
                }
                result = new UploadResult(saved, notsaved);
                log.Result = new JavaScriptSerializer().Serialize(result);
                log.Save(ttConnection);
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex.Message);
                log.Error = ex.Message;
                log.Save(ttConnection);
                return new UploadResult(0, 0);
            }
            finally
            {
                ttConnection.Close();
            }
            return result;
        }

        /// <summary>
        /// Log in to the system
        /// </summary>
        /// <param name="login">login object with just a username and password</param>
        /// <returns>login object with details of role and user id</returns>
        public Login Login(Login login)
        {
            LogEntry log = new LogEntry(getIP(), "Login", login.Name + " " + login.PW);


            string query = "SELECT Id, name, pw, email, role FROM logins";
            try
            {
                ttConnection = new SqlConnection(connection);
                ttConnection.Open();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                // return ex.Message;
            }
            //int userRole = 0;
            //int userID = 0;
            using (SqlDataAdapter loginAdapter = new SqlDataAdapter(query, ttConnection))
            {
                dataLogins = new DataTable();
                loginAdapter.Fill(dataLogins);

                int length = dataLogins.Rows.Count;
                for (int row = 0; row < length; row++)
                {
                    DataRow dr = dataLogins.Rows[row];
                    string dbname = (string)dr["name"];
                    dbname = dbname.Trim();
                    string dbpw = (string)dr["pw"];
                    dbpw = dbpw.Trim();
                    if (dbname == login.Name && dbpw == login.PW)
                    {
                        login.Role = (int)dr["role"];
                        login.ID = (int)dr["Id"];
                        break;
                    }
                }
            }
            log.Result = login.Name;
            log.Save(ttConnection);
            ttConnection.Close();
            return login;

        }

        public string Signup(Login login)
        {
            LogEntry log = new LogEntry(getIP(), "Signup", new JavaScriptSerializer().Serialize(login));


            System.Net.Mail.MailAddress emailAddr;
            string result = "OK, now please enter code from email and resubmit details";
            try
            {
                emailAddr = new System.Net.Mail.MailAddress(login.Email);
                // Valid address
            }
            catch
            {
                return ("This email address appears to be invalid");
            }
            if (login.PW.Length < 4 || login.PW.Length > 10)
                return ("Password must be between 4 and 10 characters");

            string query = "SELECT Id, name, pw, email FROM logins";
            try
            {
                ttConnection = new SqlConnection(connection);
                ttConnection.Open();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return ex.Message;
            }
            if (login.Code == 0)
            // not yet confirmed the signup
            {
                using (SqlDataAdapter loginAdapter = new SqlDataAdapter(query, ttConnection))
                {
                    dataLogins = new DataTable();
                    loginAdapter.Fill(dataLogins);

                    int length = dataLogins.Rows.Count;
                    for (int row = 0; row < length; row++)
                    {
                        DataRow dr = dataLogins.Rows[row];
                        string dbname = (string)dr["name"];
                        dbname = dbname.Trim();
                        string dbpw = (string)dr["pw"];
                        dbpw = dbpw.Trim();
                        if (dbname == login.Name)
                        {
                            result = "Sorry, this username has already been taken";
                            break;
                        }
                    }
                }
            }
            else if (login.Code == login.CalcCode())
            {
                query = string.Format("insert into logins (name, pw, email, clubID) values ('{0}','{1}','{2}','{3}')\n\r",
                    login.Name, login.PW, login.Email, 0);
                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, ttConnection))
                {
                    command.ExecuteNonQuery();
                }
                result = "Thank you, you have now registered";
            }
            else
            {
                result = "There is an error with the code number, please try again";
            }



            if (login.Code == 0)
            // not yet confirmed the signup
            {
                // create a code based on data
                login.Code = login.CalcCode();

                System.Net.Mail.MailAddress from = new System.Net.Mail.MailAddress("admin@quilkin.co.uk");
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(from, emailAddr);
                message.Subject = "BLE log signup";
                message.Body = string.Format("Please enter the code {0} into the signup page to complete your registration", login.Code);

                try
                {
                    System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(smtpserver);
                    //client.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    client.Credentials = new System.Net.NetworkCredential(smtpUserName, smtpPassword);
                    client.Send(message);
                }
                catch (Exception ex)
                {
                    result = "Sorry, there is an error with the email service: " + ex.Message;
                }
            }
            log.Result = result;
            log.Save(ttConnection);

            ttConnection.Close();

            return result;

        }

        public string SaveLocation(location loc)
        {
            LogEntry log = new LogEntry(getIP(), "SaveLocation", new JavaScriptSerializer().Serialize(loc));

            int successRows = 0;
            string result = "";
            try
            {
                ttConnection = new SqlConnection(connection);
                ttConnection.Open();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return ex.Message;

            }
            try
            {
                string T = TimeString(DateTime.Now);
                //int T = (int)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMinutes;
                //DateTime Tbase = new DateTime(2000, 1, 1);
                //int Tb = (int)TimeSpan.FromTicks(Tbase.Ticks).TotalMinutes;
                //T = T -Tb;
                string query = string.Format("insert into locations (lat,lon,dt) values ('{0}','{1}','{2}')",
                        loc.Latitude, loc.Longitude,T);


                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, ttConnection))
                {
                    successRows = command.ExecuteNonQuery();
                }
                if (successRows == 1)
                    result = string.Format("Location {0} {1} saved OK at {2}", loc.Latitude, loc.Longitude,DateTime.Now);
                else
                    result = string.Format("Database error: Location not saved");

            }

            catch (Exception ex)
            {

                Trace.WriteLine(ex.Message);
                log.Error = ex.Message;
                //return ex.Message;
            }


            finally
            {
                log.Result = result;
                log.Save(ttConnection);
                ttConnection.Close();
            }
            return result;

        }


        //        public string EmailStartSheet(int eventID)
        //        {
        //            return EmailSheet(eventID, false);
        //        }
        //        public string EmailResultSheet(int eventID)
        //        {
        //            return EmailSheet(eventID, true);
        //        }
        //        public string EmailSheet(int eventID, bool results)
        //        {
        //            try
        //            {
        //                ttConnection = new SqlConnection(connection);
        //                ttConnection.Open();
        //            }
        //            catch (Exception ex)
        //            {
        //                Trace.WriteLine(ex.Message);
        //                return ex.Message;
        //            }
        //            int emailsSent = 0;
        //            string invalid = "";
        //            string query = string.Format("SELECT riders.name,riders.email FROM  entries JOIN riders ON entries.RiderId = riders.id WHERE entries.EventId='{0}' AND riders.email is not null and riders.email!=''", eventID);
        //            try
        //            {
        //                using (SqlDataAdapter entryAdapter = new SqlDataAdapter(query, ttConnection))
        //                {
        //                    //dataEntries = new DataTable();
        //                    //entryAdapter.Fill(dataEntries);
        //                    //int length = dataEntries.Rows.Count;
        //                    ////riders = new List<Rider>();
        //                    System.Net.Mail.MailAddress from = new System.Net.Mail.MailAddress("admin@timetrials.org.uk");
        //                    System.Net.Mail.MailAddress to = new System.Net.Mail.MailAddress("admin@trurocycling.org");
        //                    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(from, to);

        //                    System.Net.Mail.MailAddress emailAddr;
        //                    System.Net.Mail.MailAddressCollection bcc = new System.Net.Mail.MailAddressCollection();
        //                    //for (int row = 0; row < length; row++)
        //                    //{
        //                    //    DataRow dr = dataEntries.Rows[row];
        //                    //    string name = (string)dr["name"];
        //                    //    string email = (string)dr["email"];

        //                    //    try
        //                    //    {
        //                    //        emailAddr = new System.Net.Mail.MailAddress(email);
        //                    //        // Valid address
        //                    //        message.Bcc.Add(emailAddr);
        //                    //    }
        //                    //    catch
        //                    //    {
        //                    //        invalid += email;
        //                    //        invalid += "\n\r";
        //                    //    }
        //                    //}

        //                    try
        //                    {
        //                        emailAddr = new System.Net.Mail.MailAddress("chrisfearnley1@gmail.com");
        //                        // Valid address
        //                        message.Bcc.Add(emailAddr);
        //                    }
        //                    catch
        //                    {
        //                        invalid += "ChrisF\n\r";
        //                    }

        //                    message.Subject = "Time Trial Entry 13 July";
        //                    message.Body = string.Format("Dear rider\n\nThank you for entering the TCC event this year");
        //                    if (results)
        //                    {
        //                        message.Body += "\nPlease find attached results (2 documents). ";
        //                    }
        //                    else
        //                    {
        //                        message.Body += "\nPlease find attached the start details. Note that event start time has been moved to 8:00am.";
        //                        message.Body += "\nPlease could you reply to this email to acknowledge receipt.";
        //                    }
        //                    message.Body += "\n\nRegards\nTruro Cycling Club";
        //                    try
        //                    {
        //                        System.Net.Mail.Attachment attach1, attach2;
        //                        if (results)
        //                        {
        //                            attach1 = new System.Net.Mail.Attachment(@"C:\Users\Chris\Documents\tcc\TCC Open 25 July 2014.pdf");
        //                            attach2 = new System.Net.Mail.Attachment(@"C:\Users\Chris\Documents\tcc\TCC Open 25 July 2014 results.pdf");
        //                        }
        //                        else
        //                        {
        //                            attach1 = new System.Net.Mail.Attachment(@"C:\Users\Chris\Documents\tcc\instructions2014.pdf");
        //                            attach2 = new System.Net.Mail.Attachment(@"C:\Users\Chris\Documents\tcc\list2014.pdf");

        //                        }
        //                        message.Attachments.Add(attach1);
        //                        message.Attachments.Add(attach2);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Trace.WriteLine(ex.Message);
        //                        return "Could not find document(s) to attach to emails";
        //                    }
        //                    try
        //                    {
        //                        System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(smtpserver);
        //                        client.Credentials = new System.Net.NetworkCredential(smtpUserName, smtpPassword);
        //                        client.Send(message);
        //                        ++emailsSent;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        return "There is an error with the email service: " + ex.Message;
        //                    }
        //                }


        //            }
        //            catch (Exception ex)
        //            {
        //                Trace.WriteLine(ex.Message);
        //                Trace.WriteLine(emailsSent + "emails sent");
        //                return ex.Message;
        //            }
        //            ttConnection.Close();
        //            if (invalid.Length > 0)
        //                return ("Emails sent but these appear invalid: " + invalid);
        //            else
        //                return ("All emails sent OK");
        //        }
    }
}

