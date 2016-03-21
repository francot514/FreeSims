using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimsLib.IFF
{
    /// <summary>
    /// Represents an stream to write to *.iff file, and use stored previous iff base.
    /// </summary>
    public class IffStream
    {
        private string m_Path, m_Name, Identifier;
        private BinaryWriter m_Writer;
        private Encoding encoding;
        public int RSMP, Size;
        public uint Version, Types;

        public IffStream(string path, Iff basefile)
            {
                m_Path = path;
                m_Name = path.Split('.')[0];
                encoding = new System.Text.ASCIIEncoding();
                m_Writer = new BinaryWriter(File.Open(m_Path, FileMode.Create), encoding);

                this.RSMP = basefile.RSMP;
                this.Size = basefile.Size;
                this.Version = basefile.Version;
                this.Types = basefile.Types;

                Identifier = "IFF FILE 2.5:TYPE FOLLOWED BY SIZE. JAMIE DOORNBOS & MAXIS 1";
            if (Identifier != "")
                 {
                byte[] idbyte = encoding.GetBytes(Identifier);

                m_Writer.Write(idbyte);

                 }

                 byte[] version = encoding.GetBytes(" ");

                 for (int i = 0; i <= basefile.Chunks.Count - 1; i++)
                     m_Writer.Write(basefile.Chunks[i].Data);

                 m_Writer.Write(basefile.Version);
                 m_Writer.Write(basefile.RSMP);
                 m_Writer.Write(basefile.Size);
                 m_Writer.Write(basefile.Types);

                 

                m_Writer.Close();
            }

        /// <summary>
        /// Represents an stream to write to *.iff file, use created chunks.
        /// </summary>
        public IffStream(string path, List<IffChunk> Chunks)
        {
            m_Path = path;
            m_Name = path.Split('.')[0];
            encoding = new System.Text.ASCIIEncoding();
            m_Writer = new BinaryWriter(File.Open(m_Path, FileMode.Create), encoding);

            Identifier = "IFF FILE 2.5:TYPE FOLLOWED BY SIZE. JAMIE DOORNBOS & MAXIS 1";
            if (Identifier != "")
            {
                byte[] idbyte = encoding.GetBytes(Identifier);

                m_Writer.Write(idbyte);

            }

            byte[] version = encoding.GetBytes(" ");

            m_Writer.Write(version);


            for (int i = 0; i <= Chunks.Count - 1; i++)
                m_Writer.Write(Chunks[i].Data);


            m_Writer.Close();
        }

    }
}
