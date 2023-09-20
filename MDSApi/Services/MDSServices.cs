using MDSWcf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSApi.Services
{
    public interface IMDSServices
    {
        Identifier GetModel(ServiceClient _clientProxy, string modelName, string versionName);
        List<string> GetAllEntities(ServiceClient _clientProxy, string modelName, string versionName);
        object GetEntityData(ServiceClient _clientProxy, string modelName, string versionName, string entity);
        void HandleOperationErrors(OperationResult result);

        string GetUserDomain(string username);
        string GetUsername(string username);
    }
    public class MDSServices : IMDSServices
    {
        public List<string> GetAllEntities(ServiceClient _clientProxy, string modelName, string versionName)
        {
            List<string> _listModel = new List<string>();
            try
            {
                // Create the request object for getting model information.
                MetadataGetRequest getRequest = new MetadataGetRequest();
                getRequest.SearchCriteria = new MetadataSearchCriteria();
                getRequest.SearchCriteria.SearchOption = SearchOption.UserDefinedObjectsOnly;

                // Set the model and version names in the search criteria.
                getRequest.SearchCriteria.Models = new System.Collections.ObjectModel.Collection<Identifier> { new Identifier { Name = modelName } };
                getRequest.SearchCriteria.Versions = new System.Collections.ObjectModel.Collection<Identifier> { new Identifier { Name = versionName } };
                getRequest.ResultOptions = new MetadataResultOptions();
                getRequest.ResultOptions.Models = ResultType.Details;

                // Get a model information.
                MetadataGetResponse getResponse = _clientProxy.MetadataGetAsync(getRequest).Result;

                var _models = getResponse.Metadata.Models.First().Entities;
                foreach (var item in _models)
                {
                    _listModel.Add(item.StagingName);
                }

                HandleOperationErrors(getResponse.OperationResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }

            return _listModel;
        }
        public object GetEntityData(ServiceClient _clientProxy, string modelName, string versionName, string entity)
        {
            string _tmpOutput = string.Empty;
            StringBuilder _sb = new StringBuilder();
            EntityMembersGetRequest getRequest = new EntityMembersGetRequest();
            EntityMembersGetResponse getResponse = new EntityMembersGetResponse();

            EntityMembersGetCriteria membersGetCriteria = new EntityMembersGetCriteria
            {
                ModelId = new Identifier() { Name = modelName },
                EntityId = new Identifier() { Name = entity },
                VersionId = new Identifier() { Name = versionName },
                MemberType = MemberType.Leaf,
                MemberReturnOption = MemberReturnOption.DataAndCounts
            };

            getRequest.MembersGetCriteria = membersGetCriteria;
            getResponse = _clientProxy.EntityMembersGetAsync(getRequest).Result;


            if (getResponse.EntityMembers.Members.Count > 0)
            {
                int _Counter0 = getResponse.EntityMembers.Members.Count - 1;

                int k = 0;
                foreach (Member individualMember in getResponse.EntityMembers.Members)
                {
                    _sb.Append("{");

                    string _appendItem = string.Format("'Code':'{0}',", individualMember.MemberId.Code);
                    _sb.Append(_appendItem);

                    _appendItem = string.Format("'Name':'{0}',", individualMember.MemberId.Name);
                    _sb.Append(_appendItem);

                    int _Max = individualMember.Attributes.Count - 1;

                    for (int i = 0; i < individualMember.Attributes.Count; i++)
                    {
                        if (i < _Max)
                        {
                            if (individualMember.Attributes[i].Type == AttributeValueType.Domain)
                            {
                                _appendItem = string.Format("'{0}':'{1}',", individualMember.Attributes[i].Identifier.Name, ((MemberIdentifier)individualMember.Attributes[i].Value).Code);
                            }
                            else
                            {
                                if (individualMember.Attributes[i].Value == null)
                                {
                                    _appendItem = string.Format("'{0}':'{1}',", individualMember.Attributes[i].Identifier.Name, string.Empty);
                                }
                                else
                                {
                                    _appendItem = string.Format("'{0}':'{1}',", individualMember.Attributes[i].Identifier.Name, individualMember.Attributes[i].Value);
                                }
                            }
                        }
                        else
                        {
                            if (individualMember.Attributes[i].Type == AttributeValueType.Domain)
                            {
                                _appendItem = string.Format("'{0}':'{1}',", individualMember.Attributes[i].Identifier.Name, ((MemberIdentifier)individualMember.Attributes[i].Value).Code);
                            }
                            else
                            {
                                if (individualMember.Attributes[i].Value == null)
                                {
                                    _appendItem = string.Format("'{0}':'{1}'", individualMember.Attributes[i].Identifier.Name, string.Empty);
                                }
                                else
                                {
                                    _appendItem = string.Format("'{0}':'{1}'", individualMember.Attributes[i].Identifier.Name, individualMember.Attributes[i].Value);
                                }
                            }

                        }

                        _sb.Append(_appendItem);
                    }

                    k++;
                    if (k <= _Counter0)
                        _sb.Append("},");
                    else
                        _sb.Append("}");
                }
            }

            _tmpOutput = "[" + _sb.ToString() + "]";

            return JsonConvert.DeserializeObject<List<ExpandoObject>>(_tmpOutput);
        }

        public Identifier GetModel(ServiceClient _clientProxy, string modelName, string versionName)
        {
            Identifier modelIdentifier = new Identifier();

            try
            {
                // Create the request object for getting model information.
                MetadataGetRequest getRequest = new MetadataGetRequest();
                getRequest.SearchCriteria = new MetadataSearchCriteria();
                getRequest.SearchCriteria.SearchOption = SearchOption.UserDefinedObjectsOnly;
                // Set the model and version names in the search criteria.
                getRequest.SearchCriteria.Models = new System.Collections.ObjectModel.Collection<Identifier> { new Identifier { Name = modelName } };
                getRequest.SearchCriteria.Versions = new System.Collections.ObjectModel.Collection<Identifier> { new Identifier { Name = versionName } };
                getRequest.ResultOptions = new MetadataResultOptions();
                getRequest.ResultOptions.Models = ResultType.Details;


                // Get a model information.
                MetadataGetResponse getResponse = _clientProxy.MetadataGetAsync(getRequest).Result;

                if (getResponse.Metadata.Models.Count > 0)
                {
                    modelIdentifier = getResponse.Metadata.Models[0].Identifier;
                }

                HandleOperationErrors(getResponse.OperationResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }

            return modelIdentifier;
        }

        public string GetUserDomain(string username)
        {
            if (username.Contains("\\") || username.Contains("/"))
            {
                username = username.Replace("\\", "/");
                string _domain = username.Split("/")[0];
                return _domain;
            }
            return string.Empty;
        }

        public string GetUsername(string username)
        {
            if (username.Contains("\\") || username.Contains("/"))
            {
                username = username.Replace("\\", "/");
                string _username = username.Split("/")[1];
                return _username;
            }
            return username;
        }

        public void HandleOperationErrors(OperationResult result)
        {
            string errorMessage = string.Empty;

            if (result.Errors.Count > 0)
            {
                foreach (Error anError in result.Errors)
                {
                    errorMessage += "Operation Error: " + anError.Code + ":" + anError.Description + "\n";
                }
                // Show the error messages.
                Console.WriteLine(errorMessage);
            }
        }
    }
}
