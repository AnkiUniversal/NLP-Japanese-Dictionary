﻿/**
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
using System.Text;

namespace Jocr
{
    public sealed class GrayImage
    {
        public byte[] Pixels { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public GrayImage() { }

        public GrayImage(int width, int height)
        {
            Width = width;
            Height = height;            
        }

        public GrayImage Clone()
        {
            GrayImage cloneImg = new GrayImage(Width, Height);
            cloneImg.Pixels = new byte[Pixels.Length];
            Pixels.CopyTo(cloneImg.Pixels, 0);
            return cloneImg;
        }
    }   
}
