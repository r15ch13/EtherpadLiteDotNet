/// <license>
///2011 Jonathon Smith
///
///Licensed under the Apache License, Version 2.0 (the "License");
///you may not use this file except in compliance with the License.
///You may obtain a copy of the License at
///
///   http://www.apache.org/licenses/LICENSE-2.0
///
///Unless required by applicable law or agreed to in writing, software
///distributed under the License is distributed on an "AS-IS" BASIS,
///WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
///See the License for the specific language governing permissions and
///limitations under the License.
/// </license>

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
        Ok,
        InvalidParameters,
        InternalError,
        InvalidFunction,
        InvalidAPIKey
    }

    public class EtherpadLiteDotNet
    {
        private const string APIVersion = "1";
        private string ApiKey { get; set; }
        private UriBuilder BaseURI { get; set; }

        public EtherpadLiteDotNet(string apiKey, string host, int port = 0)
        {
            ApiKey = apiKey;
            if (port == 0)
            {
                BaseURI = new UriBuilder("http", host);
            }
            else
            {
                BaseURI = new UriBuilder("http", host, port);
            }

        }

        private EtherpadResponse CallAPI(string functionName, string[,] query = null, Type customReturnType = null )
        {
            BaseURI.Path = "api/" + APIVersion + "/" + functionName;
            BaseURI.Query = BuildQueryString(query);

            #region Get Response And Deserialize it
            EtherpadResponse responseObject;
            using (var response = (HttpWebResponse)WebRequest.Create(BaseURI.Uri).GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                var responseText = reader.ReadToEnd();
                if (customReturnType == null)
                {
                    responseObject = js.Deserialize<EtherpadResponse>(responseText);
                }
                else
                {
                    responseObject = (EtherpadResponse)js.Deserialize(responseText,customReturnType);
                }
                responseObject.JSON = responseText;
            }
            #endregion

            #region Check for Errors In Reponse
            switch (responseObject.Code)
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
            //By calling ParseQueryString with an empty string you get an empty HttpValueCollection which cannot be created any other way as it is a private class
            var queryCollection = System.Web.HttpUtility.ParseQueryString(String.Empty);
            queryCollection.Add("apikey", ApiKey);

            if (query != null)
            {
                int queryLength = query.GetLength(0) - 1;
                for (int i = 0; i <= queryLength; i++)
                {
                    queryCollection.Add(query[i, 0], query[i, 1]);
                }
            }
        
            return queryCollection.ToString();
        }

        #region Groups

        public EtherpadResponseGroupID CreateGroup()
        {
            return (EtherpadResponseGroupID)CallAPI("createGroup", null, typeof(EtherpadResponseGroupID));
        }

        public EtherpadResponseGroupID CreateGroupIfNotExistsFor(string groupMapper)
        {
            return (EtherpadResponseGroupID)CallAPI("createGroupIfNotExistsFor",
                new string[,] { { "groupMapper", groupMapper } }, 
                typeof(EtherpadResponseGroupID));
        }

        public EtherpadResponse DeleteGroup(string groupID)
        {
            return CallAPI("deleteGroup",
                new string[,] { { "groupID", groupID } });
        }

        public EtherpadResponsePadIDs ListPads(string groupID)
        {
            return (EtherpadResponsePadIDs)CallAPI("listPads",
                new string[,] { { "groupID", groupID } }, 
                typeof(EtherpadResponsePadIDs));
        }

        public EtherpadResponse CreateGroupPad(string groupID, string padName)
        {
            return CallAPI("createGroupPad",
                new string[,] { { "groupID", groupID }, { "padName", padName } });
        }

        public EtherpadResponse CreateGroupPad(string groupID, string padName, string text)
        {
            return CallAPI("createGroupPad",
                new string[,] { { "groupID", groupID }, { "padName", padName }, { "text", text } });
        }

        #endregion

        #region Author

        public EtherpadResponse CreateAuthor()
        {
            return CallAPI("createAuthor", null, typeof(EtherpadResponseAuthorID));
        }

        public EtherpadResponseAuthorID CreateAuthor(string name)
        {
            return (EtherpadResponseAuthorID)CallAPI("createAuthor",
                new string[,] { { "name", name } },
                typeof(EtherpadResponseAuthorID));
        }

        public EtherpadResponseAuthorID CreateAuthorIfNotExistsFor(string authorMapper)
        {
            return (EtherpadResponseAuthorID)CallAPI("createAuthorIfNotExistsFor",
                new string[,] { { "authorMapper", authorMapper } },
                typeof(EtherpadResponseAuthorID));
        }

        public EtherpadResponseAuthorID CreateAuthorIfNotExistsFor(string authorMapper, string name)
        {
            return (EtherpadResponseAuthorID)CallAPI("createAuthorIfNotExistsFor",
                new string[,] { { "authorMapper", authorMapper }, { "name", name } },
                typeof(EtherpadResponseAuthorID));
        }

        #endregion

        #region Session

        public EtherpadResponseSessionID CreateSession(string groupID, string authorID, string validUntil)
        {
            return (EtherpadResponseSessionID)CallAPI("createSession",
                new string[,] { { "groupID", groupID }, { "authorID ", authorID }, { "validUntil", validUntil } },
                typeof(EtherpadResponseSessionID));
        }

        public EtherpadResponse DeleteSession(string sessionID)
        {
            return CallAPI("deleteSession",
                new string[,] { { "sessionID", sessionID } });
        }

        public EtherpadResponse GetSessionInfo(string sessionID)
        {
            return CallAPI("getSessionInfo",
                new string[,] { { "sessionID", sessionID } });
        }

        public EtherpadResponseSessionInfos ListSessionsOfGroup(string groupID) 
        {
            return (EtherpadResponseSessionInfos)CallAPI("listSessionsOfGroup",
                new string[,] { { "groupID", groupID } },
                typeof(EtherpadResponseSessionInfos));
        }

        public EtherpadResponseSessionInfos ListSessionsOfAuthor(string authorID)
        {
            return (EtherpadResponseSessionInfos)CallAPI("listSessionsOfAuthor",
                new string[,] { { "authorID", authorID } },
                typeof(EtherpadResponseSessionInfos));
        }

        #endregion

        #region Pad

        public EtherpadResponsePadText GetText(string padID)
        {
            return (EtherpadResponsePadText)CallAPI("getText",
                new string[,] { { "padID", padID } },
                typeof(EtherpadResponsePadText));
        }

        public EtherpadResponsePadText GetText(string padID, int rev)
        {
            return (EtherpadResponsePadText)CallAPI("getText",
                new string[,] { { "padID", padID }, { "rev", rev.ToString() } },
                typeof(EtherpadResponsePadText));
        }

        public EtherpadResponse SetText(string padID, string text)
        {
            return CallAPI("setText",
                new string[,] { { "padID", padID }, { "text", text } });
        }

        public EtherpadResponse CreatePad(string padID)
        {
            return CallAPI("createPad", 
                new string[,] {{"padID", padID}} );
        }

        public EtherpadResponse CreatePad(string padID, string text)
        {
            return CallAPI("createPad", 
                new string[,] { { "padID", padID } , { "text", text } });
        }

        public EtherpadResponsePadRevisions GetRevisionsCount(string padID)
        {
            return (EtherpadResponsePadRevisions)CallAPI("getRevisionsCount", 
                new string[,] { { "padID", padID } },
                typeof(EtherpadResponsePadRevisions));
        }

        public EtherpadResponse DeletePad(string padID)
        {
            return CallAPI("deletePad", 
                new string[,] { { "padID", padID } });
        }

        public EtherpadResponsePadReadOnlyID GetReadOnlyID(string padID)
        {
            return (EtherpadResponsePadReadOnlyID)CallAPI("getReadOnlyID",
                   new string[,] { { "padID", padID } },
                   typeof(EtherpadResponsePadReadOnlyID));
        }

        public EtherpadResponse SetPublicStatus(string padID, bool publicStatus)
        {
            return CallAPI("setPublicStatus",
                    new string[,] { { "padID", padID }, { "publicStatus", publicStatus.ToString() } });
        }

        public EtherpadResponsePadPublicStatus GetPublicStatus(string padID)
        {
            return (EtherpadResponsePadPublicStatus)CallAPI("getPublicStatus",
                   new string[,] { { "padID", padID } },
                   typeof(EtherpadResponsePadPublicStatus));
        }

        public EtherpadResponse SetPassword(string padID, string password)
        {
            return CallAPI("setPassword",
                   new string[,] { { "padID", padID } , { "password", password } });
        }

        public EtherpadResponsePadPasswordProtection IsPasswordProtected(string padID)
        {
            return (EtherpadResponsePadPasswordProtection)CallAPI("isPasswordProtected",
                   new string[,] { { "padID", padID } },
                   typeof(EtherpadResponsePadPasswordProtection));
        }

        #endregion Pad

    }

    /// <summary>
    ///This class is returned by all the API calls
    ///If you wanted to reduce the number of classes needed
    ///The strong typing could be replaced by: public Dictionary<string,string> Data { get; set; } in the base class
    /// </summary>
    public class EtherpadResponse
    {
        public EtherpadReturnCodeEnum Code { get; set; } 
        public string Message { get; set; }        
        public string JSON { get; set; }
    }

    #region Classes to Strong Type Response

    public class EtherpadResponseGroupID : EtherpadResponse
    {
        public DataGroupID Data {get; set;}
    }

    public class DataGroupID
    {
        public string GroupID { get; set; } 
    }

    public class EtherpadResponsePadIDs : EtherpadResponse
    {
        public DataPadIDs Data { get; set; }
    }

    public class DataPadIDs
    {
        public IEnumerable<string> PadIDs { get; set; }
    }

    public class EtherpadResponseAuthorID : EtherpadResponse
    {
        public DataAuthorID Data { get; set; }
    }

    public class DataAuthorID
    {
        public string AuthorID { get; set; }
    }

    public class EtherpadResponseSessionID : EtherpadResponse
    {
        public DataSessionID Data { get; set; }
    }

    public class DataSessionID
    {
        public string SessionID { get; set; }
    }

    public class EtherpadResponseSessionInfos : EtherpadResponse
    {
        public IEnumerable<DataSessionInfo> Data { get; set; }
    }

    public class DataSessionInfo
    {
        public string GrouopID { get; set; }
        public string AuthorID { get; set; }
        public int ValidUntil { get; set; }
    }

    public class EtherpadResponsePadText : EtherpadResponse
    {
        public DataPadText Data { get; set; }
    }

    public class DataPadText
    {
        public string Text { get; set; }
    }

    public class EtherpadResponsePadRevisions : EtherpadResponse
    {
        public DataPadRevisions Data { get; set; }
    }

    public class DataPadRevisions
    {
        public int Revisions { get; set; }
    }

    public class EtherpadResponsePadReadOnlyID : EtherpadResponse
    {
        public DataPadReadOnlyID Data { get; set; }
    }

    public class DataPadReadOnlyID
    {
        public string ReadOnlyID { get; set; }
    }

    public class EtherpadResponsePadPublicStatus : EtherpadResponse
    {
        public DataPadPublicStatus Data { get; set; }
    }

    public class DataPadPublicStatus
    {
        public bool PublicStatus { get; set; }
    }

    public class EtherpadResponsePadPasswordProtection : EtherpadResponse
    {
        public DataPadPasswordProtection Data { get; set; }
    }

    public class DataPadPasswordProtection
    {
        public bool PasswordProtection { get; set; }
    }

    #endregion

}

