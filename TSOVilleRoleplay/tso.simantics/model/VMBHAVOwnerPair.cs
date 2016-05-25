/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Files.formats.iff.chunks;
using TSO.Content;

namespace TSO.SimsAntics.Model
{
    public class VMBHAVOwnerPair
    {
        public BHAV bhav;
        public GameObject owner;

        public VMBHAVOwnerPair(BHAV bhav, GameObject owner)
        {
            this.bhav = bhav;
            this.owner = owner;
        }
    }
}
