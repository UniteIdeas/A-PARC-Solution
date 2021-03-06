﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TextClassification.Service.FrogProxyWebService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://Web.FrogProxy/", ConfigurationName="FrogProxyWebService.FrogProxyWebService")]
    public interface FrogProxyWebService {
        
        // CODEGEN: Generating message contract since element name Input from namespace  is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://Web.FrogProxy/FrogProxyWebService/GetStemRequest", ReplyAction="http://Web.FrogProxy/FrogProxyWebService/GetStemResponse")]
        TextClassification.Service.FrogProxyWebService.GetStemResponse GetStem(TextClassification.Service.FrogProxyWebService.GetStemRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Web.FrogProxy/FrogProxyWebService/GetStemRequest", ReplyAction="http://Web.FrogProxy/FrogProxyWebService/GetStemResponse")]
        System.Threading.Tasks.Task<TextClassification.Service.FrogProxyWebService.GetStemResponse> GetStemAsync(TextClassification.Service.FrogProxyWebService.GetStemRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class GetStemRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="GetStem", Namespace="http://Web.FrogProxy/", Order=0)]
        public TextClassification.Service.FrogProxyWebService.GetStemRequestBody Body;
        
        public GetStemRequest() {
        }
        
        public GetStemRequest(TextClassification.Service.FrogProxyWebService.GetStemRequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="")]
    public partial class GetStemRequestBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string Input;
        
        public GetStemRequestBody() {
        }
        
        public GetStemRequestBody(string Input) {
            this.Input = Input;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class GetStemResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="GetStemResponse", Namespace="http://Web.FrogProxy/", Order=0)]
        public TextClassification.Service.FrogProxyWebService.GetStemResponseBody Body;
        
        public GetStemResponse() {
        }
        
        public GetStemResponse(TextClassification.Service.FrogProxyWebService.GetStemResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="")]
    public partial class GetStemResponseBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string @return;
        
        public GetStemResponseBody() {
        }
        
        public GetStemResponseBody(string @return) {
            this.@return = @return;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface FrogProxyWebServiceChannel : TextClassification.Service.FrogProxyWebService.FrogProxyWebService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class FrogProxyWebServiceClient : System.ServiceModel.ClientBase<TextClassification.Service.FrogProxyWebService.FrogProxyWebService>, TextClassification.Service.FrogProxyWebService.FrogProxyWebService {
        
        public FrogProxyWebServiceClient() {
        }
        
        public FrogProxyWebServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public FrogProxyWebServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public FrogProxyWebServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public FrogProxyWebServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        TextClassification.Service.FrogProxyWebService.GetStemResponse TextClassification.Service.FrogProxyWebService.FrogProxyWebService.GetStem(TextClassification.Service.FrogProxyWebService.GetStemRequest request) {
            return base.Channel.GetStem(request);
        }
        
        public string GetStem(string Input) {
            TextClassification.Service.FrogProxyWebService.GetStemRequest inValue = new TextClassification.Service.FrogProxyWebService.GetStemRequest();
            inValue.Body = new TextClassification.Service.FrogProxyWebService.GetStemRequestBody();
            inValue.Body.Input = Input;
            TextClassification.Service.FrogProxyWebService.GetStemResponse retVal = ((TextClassification.Service.FrogProxyWebService.FrogProxyWebService)(this)).GetStem(inValue);
            return retVal.Body.@return;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<TextClassification.Service.FrogProxyWebService.GetStemResponse> TextClassification.Service.FrogProxyWebService.FrogProxyWebService.GetStemAsync(TextClassification.Service.FrogProxyWebService.GetStemRequest request) {
            return base.Channel.GetStemAsync(request);
        }
        
        public System.Threading.Tasks.Task<TextClassification.Service.FrogProxyWebService.GetStemResponse> GetStemAsync(string Input) {
            TextClassification.Service.FrogProxyWebService.GetStemRequest inValue = new TextClassification.Service.FrogProxyWebService.GetStemRequest();
            inValue.Body = new TextClassification.Service.FrogProxyWebService.GetStemRequestBody();
            inValue.Body.Input = Input;
            return ((TextClassification.Service.FrogProxyWebService.FrogProxyWebService)(this)).GetStemAsync(inValue);
        }
    }
}
