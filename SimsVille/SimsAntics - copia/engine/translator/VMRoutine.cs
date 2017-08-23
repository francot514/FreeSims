﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Files.formats.iff;
using TSO.Files.formats.iff.chunks;

namespace TSO.SimsAntics
{
    public class VMRoutine
    {
        public VMRoutine(){
        }

        public VM VM;
        public byte Type;
        public VMInstruction[] Instructions;
        public ushort Locals;
        public ushort Arguments;
        public ushort ID;

        /** Run time info **/
        public VMFunctionRTI Rti;

        public BHAV Chunk;
        public uint RuntimeVer;
    }


    public class VMFunctionRTI
    {
        public string Name;
    }
}
