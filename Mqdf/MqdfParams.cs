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
using System.Reflection;
using System.IO;

namespace Mqdf
{
    public static class MqdfParams
    {
        public static Stream GetLdaVectorStream()
        {
            return GetResource("ldaeig.bin");
        }

        public static Stream GetMqdfStream()
        {
            return GetResource("qdf.bin");
        }

        private static Stream GetResource(string resourceName)
        {
            Assembly assembly = typeof(MqdfParams).GetTypeInfo().Assembly;
            string[] names = assembly.GetManifestResourceNames();
            foreach (var name in names)
            {
                if (name.Contains(resourceName))
                    return assembly.GetManifestResourceStream(name);
            }
            return null;
        }
    }
}
