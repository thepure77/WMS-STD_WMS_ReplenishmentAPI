using System;

namespace Business.Commons
{
    class Logtxt
    {
        public void LoggingErr(String Folder, String msg)
        {
            var PathLog = System.IO.Directory.GetCurrentDirectory();
            if (Folder != "")
            {
                PathLog += "\\log_Err" + Folder + "\\";
            }
            else
            {
                PathLog += "\\NoFolder\\";
            }

            try
            {


                //var path = Directory.GetCurrentDirectory();
                //path += "\\" + floder.ToString() + "\\";

                if (!System.IO.Directory.Exists(PathLog))
                {
                    System.IO.Directory.CreateDirectory(PathLog);

                }
                String FileName = "log_Err" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".txt";

                String logmsg = DateTime.Now.ToString("yyyy-MM-dd-HHmmss ") + " -- " + msg + Environment.NewLine;


                System.IO.File.AppendAllText(PathLog + FileName, logmsg);

            }
            catch (Exception ex)
            {
                string s = ex.Message;
                //  lblerr.Text = ex.Message;
            }
            finally
            {

            }
        }
        public void Logging(String Folder, String msg)
        {
            var PathLog = System.IO.Directory.GetCurrentDirectory();
            if (Folder != "")
            {
                PathLog += "\\log_" + Folder + "\\";
            }
            else
            {
                PathLog += "\\NoFolder\\";
            }

            try
            {


                //var path = Directory.GetCurrentDirectory();
                //path += "\\" + floder.ToString() + "\\";

                if (!System.IO.Directory.Exists(PathLog))
                {
                    System.IO.Directory.CreateDirectory(PathLog);

                }
                String FileName = "log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                String logmsg = DateTime.Now.ToString("yyyy-MM-dd-HHmmss ") + " -- " + msg + Environment.NewLine;


                System.IO.File.AppendAllText(PathLog + FileName, logmsg);

            }
            catch (Exception ex)
            {
                string s = ex.Message;
                //  lblerr.Text = ex.Message;
            }
            finally
            {

            }
        }

    }
}
