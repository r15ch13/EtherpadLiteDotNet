using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web.Script.Serialization;

namespace Etherpad
{
    public enum EtherpadReturnCodeEnum
    {
        OK,
        InvalidParameters,
        InternalError,
        InvalidFunction,
        InvalidAPIKey
    }

    public class EtherpadLiteDotNet
    {
        private const string APIVersion = "1";
        private string APIKey { get; set; }
        private UriBuilder BaseURI { get; set; }

        //By calling ParseQueryString with an empty string you get an empty HttpValueCollection which cannot be created any other way as it is a private class
        private NameValueCollection QueryStringBase = System.Web.HttpUtility.ParseQueryString(String.Empty);

        public EtherpadLiteDotNet(string apiKey, string host, int port = 0)
        {
            QueryStringBase.Add("apikey", apiKey);
            if (port == 0)
            {
                BaseURI = new UriBuilder("http", host);
            }
            else
            {
                BaseURI = new UriBuilder("http", host, port);
            }

        }

        private EtherpadResponse CallAPI(string functionName, string query = "")
        {
            BaseURI.Path = "api/" + APIVersion + "/" + functionName;
            BaseURI.Query = query;

            #region Get Response And Deserialize it
            EtherpadResponse responseObject;
            using (var response = (HttpWebResponse)WebRequest.Create(BaseURI.Uri).GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                var responseText = reader.ReadToEnd();
                responseObject = js.Deserialize<EtherpadResponse>(responseText);
                responseObject.JSON = responseText;
            }
            #endregion

            #region Check for Errors In Reponse
            switch (responseObject.ReturnCode)
            {
                case EtherpadReturnCodeEnum.InternalError:
                    throw new SystemException("An error has occured in Etherpad: " + responseObject.Message);
                case EtherpadReturnCodeEnum.InvalidAPIKey:
                    throw new ArgumentException("The API key supplied is invalid.");
                case EtherpadReturnCodeEnum.InvalidFunction:
                    throw new MissingMethodException("The function name passed is invalid.", responseObject.Message);
                case EtherpadReturnCodeEnum.InvalidParameters:
                    throw new ArgumentException("An invalid parameter has been passed to the function.", responseObject.Message);
            }
            #endregion

            return responseObject;
        }

        private string BuildQueryString(string[,] query)
        {
            var queryCollection = QueryStringBase;
            int queryLength = query.Length - 1;
            for (int i = 0; i < queryLength; i++)
            {
                queryCollection.Add(query[i, 0], query[i, 1]);
            }
            return queryCollection.ToString();
        }

        #region Groups

        public EtherpadResponse CreateGroup()
        {
            return CallAPI("createGroup");
        }

        public EtherpadResponse CreateGroupIfNotExistsFor(string groupMapper)
        {
            return CallAPI("createGroupIfNotExistsFor",
                BuildQueryString(new string[,] { { "groupMapper", groupMapper } }));
        }

        public EtherpadResponse DeleteGroup(string groupID)
        {
            return CallAPI("deleteGroup",
                BuildQueryString(new string[,] { { "groupID", groupID } }));
        }

        public EtherpadResponse ListPads(string groupID)
        {
            return CallAPI("listPads",
                BuildQueryString(new string[,] { { "groupID", groupID } }));
        }

        public EtherpadResponse CreateGroupPad(string groupID, string padName)
        {
            return CallAPI("createGroupPad",
                BuildQueryString(new string[,] { { "groupID", groupID }, { "padName ", padName } }));
        }

        public EtherpadResponse CreateGroupPad(string groupID, string padName, string text)
        {
            return CallAPI("createGroupPad",
                BuildQueryString(new string[,] { { "groupID", groupID }, { "padName ", padName }, { "text", text } }));
        }

        #endregion

        #region Author

        public EtherpadResponse CreateAuthor()
        {
            return CallAPI("createAuthor");
        }

        public EtherpadResponse CreateAuthor(string name)
        {
            return CallAPI("createAuthor",
                BuildQueryString(new string[,] { { "name", name } }));
        }

        public EtherpadResponse CreateAuthorIfNotExistsFor(string authorMapper)
        {
            return CallAPI("createAuthorIfNotExistsFor",
                BuildQueryString(new string[,] { { "authorMapper", authorMapper } }));
        }

        public EtherpadResponse CreateAuthorIfNotExistsFor(string authorMapper, string name)
        {
            return CallAPI("createAuthorIfNotExistsFor",
                BuildQueryString(new string[,] { { "authorMapper", authorMapper }, { "name", name } }));
        }

        #endregion

        #region Session

        public EtherpadResponse CreateSession(string groupID, string authorID, string validUntil)
        {
            return CallAPI("createSession",
                BuildQueryString(new string[,] { { "groupID", groupID }, { "authorID ", authorID }, { "validUntil", validUntil } }));
        }

        public EtherpadResponse DeleteSession(string sessionID)
        {
            return CallAPI("deleteSession",
                BuildQueryString(new string[,] { { "sessionID", sessionID } }));
        }

        public EtherpadResponse ListSessionsOfGroup(string groupID) 
        {
            return CallAPI("listSessionsOfGroup",
                BuildQueryString(new string[,] { { "groupID", groupID } }));
        }

        public EtherpadResponse ListSessionsOfAuthor(string authorID)
        {
            return CallAPI("listSessionsOfAuthor",
                BuildQueryString(new string[,] { { "authorID", authorID } }));
        }

        #endregion

        #region Pad

        public EtherpadResponse GetText(string padID)
        {
            return CallAPI("getText",
                BuildQueryString(new string[,] { { "padID", padID } }));
        }

        public EtherpadResponse GetText(string padID, int rev)
        {
            return CallAPI("getText",
                BuildQueryString(new string[,] { { "padID", padID }, { "rev", rev.ToString() } }));
        }

        public EtherpadResponse SetText(string padID, string text)
        {
            return CallAPI("setText",
                BuildQueryString(new string[,] { { "padID", padID }, { "text", text } }));
        }

        public EtherpadResponse CreatePad(string padID)
        {
            return CallAPI("createPad", 
                BuildQueryString(new string[,] {{"padID", padID}} ));
        }

        public EtherpadResponse CreatePad(string padID, string text)
        {
            return CallAPI("createPad", 
                BuildQueryString(new string[,] { { "padID", padID } , { "text", text } }));
        }

        public EtherpadResponse GetRevisionsCount(string padID)
        {
            return CallAPI("getRevisionsCount", 
                BuildQueryString(new string[,] { { "padID", padID } }));
        }

        public EtherpadResponse DeletePad(string padID)
        {
            return CallAPI("deletePad", 
                BuildQueryString(new string[,] { { "padID", padID } }));
        }

        public EtherpadResponse GetReadOnlyID(string padID)
        {
            return CallAPI("getReadOnlyID",
                   BuildQueryString(new string[,] { { "padID", padID } }));
        }

        public EtherpadResponse SetPublicStatus(string padID, bool publicStatus)
        {
            return CallAPI("setPublicStatus",
                    BuildQueryString(new string[,] { { "padID", padID }, { "publicStatus", publicStatus.ToString() } }));
        }

        public EtherpadResponse GetPublicStatus(string padID)
        {
            return CallAPI("getPublicStatus",
                   BuildQueryString(new string[,] { { "padID", padID } }));
        }

        public EtherpadResponse SetPassword(string padID, string password)
        {
            return CallAPI("setPassword",
                   BuildQueryString(new string[,] { { "padID", padID } , { "password", password } }));
        }

        public EtherpadResponse IsPasswordProtected(string padID)
        {
            return CallAPI("isPasswordProtected",
                   BuildQueryString(new string[,] { { "padID", padID } }));
        }

        #endregion Pad

    }

    //This class is returned by all the API calls
    //The data is maped to a <string,string> dictionary, in the future I may implement full strong typing.
    public class EtherpadResponse
    {
        public EtherpadReturnCodeEnum ReturnCode {get; set;}
        public string Message { get; set; }
        public Dictionary<string,string> Data { get; set; }
        public string JSON { get; set; }
    }
}

