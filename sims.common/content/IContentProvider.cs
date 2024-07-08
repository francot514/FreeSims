﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSO.Common.Content
{
    public interface IContentProvider <T>
    {
        T Get(ulong id, bool ts1);
        T Get(uint type, uint fileID, bool ts1);
        T Get(ContentID id);
        T Get(string name);
        List<IContentReference<T>> List();
    }
}
