/**
 * Copyright © 2017-2018 Anki Universal Team.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.  A copy of the
 * License is distributed with this work in the LICENSE.md file.  You may
 * also obtain a copy of the License from
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NLPJapaneseDictionary.NetTcp
{
    public class NetTcpHelper
    {
        internal const string BASE_ADDRESS = "net.pipe://localhost/AnkiUniversalNlpJapaneseDictionaryService";
        internal const string SERVICE = "NlpJDictService";
        internal const string END_POINT_ADDRESS = BASE_ADDRESS + "/" + SERVICE;

        public static ServiceHost CreateNetNamedPipeServer(SearchTextHandler handler)
        {
            try
            {
                var serviceHost = new ServiceHost(typeof(NlpJdictService), new Uri[] { new Uri(BASE_ADDRESS) });

                var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                binding.TransferMode = TransferMode.Streamed;

                serviceHost.AddServiceEndpoint(typeof(INlpJdictService), binding, SERVICE);
                NlpJdictService.Handler = handler;
                serviceHost.Open();
                return serviceHost;
            }
            catch
            {
                return null;
            }
        }
    }

    public class NlpJdictServiceProxy : ClientBase<INlpJdictService>
    {                
        public NlpJdictServiceProxy()
            : base(new ServiceEndpoint(ContractDescription.GetContract(typeof(INlpJdictService)),
                new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { TransferMode = TransferMode.Streamed }, 
                                        new EndpointAddress(NetTcpHelper.END_POINT_ADDRESS)))
        {

        }

        public void SearchText(string text)
        {
            Channel.SearchText(text);
        }
    }
}
