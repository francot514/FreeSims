// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Tables.EGT
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable disable
namespace GOLDEngine.Tables;

internal class EGT
{
  private GrammarProperties m_Grammar = new GrammarProperties();
  private SymbolList m_SymbolTable;
  private FAStateList m_DFA;
  private CharacterSetList m_CharSetTable;
  private ProductionList m_ProductionTable;
  private LRStateList m_LRStates;
  private GroupList m_GroupTable;
  private Dictionary<Symbol, Group> m_GroupStart = new Dictionary<Symbol, Group>();

  internal short InitialLRState => this.m_LRStates.InitialState;

  internal short InitialDFAState => this.m_DFA.InitialState;

  internal GrammarProperties Grammar => this.m_Grammar;

  internal ProductionList ProductionTable => this.m_ProductionTable;

  internal SymbolList SymbolTable => this.m_SymbolTable;

  internal Group GetGroup(Symbol start) => this.m_GroupStart[start];

  internal LRAction FindLRAction(short CurrentLALR, Symbol symbolToFind)
  {
    return this.m_LRStates[(int) CurrentLALR].Find((Predicate<LRAction>) (action => action.Symbol.Equals((object) symbolToFind)));
  }

  internal Production GetProduction(LRAction ParseAction)
  {
    return this.m_ProductionTable[(int) ParseAction.Value];
  }

  internal SymbolList GetExpectedSymbols(short CurrentLALR)
  {
    LRState lrState = this.m_LRStates[(int) CurrentLALR];
    List<Symbol> expectedSymbols = new List<Symbol>();
    Action<LRAction> action1 = (Action<LRAction>) (action =>
    {
      switch (action.Symbol.Type)
      {
        case SymbolType.Content:
        case SymbolType.End:
        case SymbolType.GroupStart:
        case SymbolType.GroupEnd:
          expectedSymbols.Add(action.Symbol);
          break;
      }
    });
    lrState.ForEach(action1);
    return new SymbolList(expectedSymbols);
  }

  internal FAState GetFAState(short CurrentDFA) => this.m_DFA[(int) CurrentDFA];

  internal Symbol GetFirstSymbolOfType(SymbolType symbolTypeToFind)
  {
    return this.m_SymbolTable.GetFirstOfType(symbolTypeToFind);
  }

  internal EGT(BinaryReader Reader)
  {
    EGT.EGTReader egtReader = new EGT.EGTReader(Reader);
    try
    {
      while (!egtReader.EndOfFile())
      {
        egtReader.GetNextRecord();
        EGT.EGTRecord egtRecord = (EGT.EGTRecord) egtReader.RetrieveByte();
        if ((uint) egtRecord <= 82U)
        {
          if ((uint) egtRecord <= 73U)
          {
            switch (egtRecord)
            {
              case EGT.EGTRecord.DFAState:
                int index1 = (int) egtReader.RetrieveInt16();
                int num1 = egtReader.RetrieveBoolean() ? 1 : 0;
                int Index1 = (int) egtReader.RetrieveInt16();
                egtReader.RetrieveEntry();
                if (num1 != 0)
                  this.m_DFA[index1] = new FAState(this.m_SymbolTable[Index1]);
                else
                  this.m_DFA[index1] = new FAState();
                while (!egtReader.RecordComplete())
                {
                  int index2 = (int) egtReader.RetrieveInt16();
                  short Target = egtReader.RetrieveInt16();
                  egtReader.RetrieveEntry();
                  this.m_DFA[index1].Edges.Add(new FAEdge(this.m_CharSetTable[index2], Target));
                }
                continue;
              case EGT.EGTRecord.InitialStates:
                this.m_DFA.InitialState = egtReader.RetrieveInt16();
                this.m_LRStates.InitialState = egtReader.RetrieveInt16();
                continue;
            }
          }
          else
          {
            switch (egtRecord)
            {
              case EGT.EGTRecord.LRState:
                int index3 = (int) egtReader.RetrieveInt16();
                egtReader.RetrieveEntry();
                this.m_LRStates[index3] = new LRState();
                while (!egtReader.RecordComplete())
                {
                  int Index2 = (int) egtReader.RetrieveInt16();
                  LRActionType Type = (LRActionType) egtReader.RetrieveInt16();
                  short num2 = egtReader.RetrieveInt16();
                  egtReader.RetrieveEntry();
                  this.m_LRStates[index3].Add(new LRAction(this.m_SymbolTable[Index2], Type, num2));
                }
                continue;
              case EGT.EGTRecord.Production:
                short num3 = egtReader.RetrieveInt16();
                int Index3 = (int) egtReader.RetrieveInt16();
                egtReader.RetrieveEntry();
                List<Symbol> symbols = new List<Symbol>();
                while (!egtReader.RecordComplete())
                {
                  int Index4 = (int) egtReader.RetrieveInt16();
                  symbols.Add(this.m_SymbolTable[Index4]);
                }
                SymbolList Handle = new SymbolList(symbols);
                this.m_ProductionTable[(int) num3] = new Production(this.m_SymbolTable[Index3], num3, Handle);
                continue;
            }
          }
        }
        else if ((uint) egtRecord <= 99U)
        {
          switch (egtRecord)
          {
            case EGT.EGTRecord.Symbol:
              short num4 = egtReader.RetrieveInt16();
              string Name = egtReader.RetrieveString();
              SymbolType Type1 = (SymbolType) egtReader.RetrieveInt16();
              this.m_SymbolTable[(int) num4] = new Symbol(Name, Type1, num4);
              continue;
            case EGT.EGTRecord.CharRanges:
              int index4 = (int) egtReader.RetrieveInt16();
              int num5 = (int) egtReader.RetrieveInt16();
              int num6 = (int) egtReader.RetrieveInt16();
              egtReader.RetrieveEntry();
              this.m_CharSetTable[index4] = new CharacterSet();
              while (!egtReader.RecordComplete())
                this.m_CharSetTable[index4].Add(new CharacterRange(egtReader.RetrieveUInt16(), egtReader.RetrieveUInt16()));
              continue;
          }
        }
        else
        {
          switch (egtRecord)
          {
            case EGT.EGTRecord.Group:
              Group group = new Group();
              group.TableIndex = egtReader.RetrieveInt16();
              group.Name = egtReader.RetrieveString();
              group.Container = this.m_SymbolTable[(int) egtReader.RetrieveInt16()];
              group.Start = this.m_SymbolTable[(int) egtReader.RetrieveInt16()];
              group.End = this.m_SymbolTable[(int) egtReader.RetrieveInt16()];
              group.Advance = (Group.AdvanceMode) egtReader.RetrieveInt16();
              group.Ending = (Group.EndingMode) egtReader.RetrieveInt16();
              egtReader.RetrieveEntry();
              int num7 = (int) egtReader.RetrieveInt16();
              for (int index5 = 1; index5 <= num7; ++index5)
                group.Nesting.Add((int) egtReader.RetrieveInt16());
              this.m_GroupStart.Add(group.Start, group);
              this.m_GroupTable[(int) group.TableIndex] = group;
              continue;
            case EGT.EGTRecord.Property:
              int Index5 = (int) egtReader.RetrieveInt16();
              egtReader.RetrieveString();
              this.m_Grammar.SetValue(Index5, egtReader.RetrieveString());
              continue;
            case EGT.EGTRecord.TableCounts:
              this.m_SymbolTable = new SymbolList((int) egtReader.RetrieveInt16());
              this.m_CharSetTable = new CharacterSetList((int) egtReader.RetrieveInt16());
              this.m_ProductionTable = new ProductionList((int) egtReader.RetrieveInt16());
              this.m_DFA = new FAStateList((int) egtReader.RetrieveInt16());
              this.m_LRStates = new LRStateList((int) egtReader.RetrieveInt16());
              this.m_GroupTable = new GroupList((int) egtReader.RetrieveInt16());
              continue;
          }
        }
        throw new ParserException($"File Error. A record of type '{((char) egtRecord).ToString()}' was read. This is not a valid code.");
      }
    }
    catch (Exception ex)
    {
      throw new ParserException(ex.Message, ex, "LoadTables");
    }
  }

  internal enum EGTRecord : byte
  {
    DFAState = 68, // 0x44
    InitialStates = 73, // 0x49
    LRState = 76, // 0x4C
    Production = 82, // 0x52
    Symbol = 83, // 0x53
    CharRanges = 99, // 0x63
    Group = 103, // 0x67
    Property = 112, // 0x70
    TableCounts = 116, // 0x74
  }

  internal class EGTReader
  {
    private const byte kRecordContentMulti = 77;
    private string m_FileHeader;
    private BinaryReader m_Reader;
    private ushort m_EntryCount;
    private ushort m_EntriesRead;

    internal EGTReader(BinaryReader Reader)
    {
      this.m_Reader = Reader;
      this.m_EntryCount = (ushort) 0;
      this.m_EntriesRead = (ushort) 0;
      this.m_FileHeader = this.RawReadCString();
    }

    public bool RecordComplete() => (int) this.m_EntriesRead >= (int) this.m_EntryCount;

    public ushort EntryCount() => this.m_EntryCount;

    public bool EndOfFile() => this.m_Reader.BaseStream.Position == this.m_Reader.BaseStream.Length;

    public string Header() => this.m_FileHeader;

    public EGT.EGTReader.Entry RetrieveEntry()
    {
      EGT.EGTReader.Entry entry = new EGT.EGTReader.Entry();
      if (this.RecordComplete())
      {
        entry.Type = EGT.EGTReader.EntryType.Empty;
        entry.Value = (object) "";
      }
      else
      {
        ++this.m_EntriesRead;
        byte num1 = this.m_Reader.ReadByte();
        entry.Type = (EGT.EGTReader.EntryType) num1;
        EGT.EGTReader.EntryType type = entry.Type;
        if ((uint) type <= 69U)
        {
          switch (type)
          {
            case EGT.EGTReader.EntryType.Boolean:
              byte num2 = this.m_Reader.ReadByte();
              entry.Value = (object) (num2 == (byte) 1);
              goto label_11;
            case EGT.EGTReader.EntryType.Empty:
              entry.Value = (object) "";
              goto label_11;
          }
        }
        else
        {
          switch (type)
          {
            case EGT.EGTReader.EntryType.UInt16:
              entry.Value = (object) this.RawReadUInt16();
              goto label_11;
            case EGT.EGTReader.EntryType.String:
              entry.Value = (object) this.RawReadCString();
              goto label_11;
            case EGT.EGTReader.EntryType.Byte:
              entry.Value = (object) this.m_Reader.ReadByte();
              goto label_11;
          }
        }
        entry.Type = EGT.EGTReader.EntryType.Error;
        entry.Value = (object) "";
      }
label_11:
      return entry;
    }

    private ushort RawReadUInt16()
    {
      int num = (int) this.m_Reader.ReadByte();
      return (ushort) (((int) this.m_Reader.ReadByte() << 8) + num);
    }

    private string RawReadCString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      bool flag = false;
      while (!flag)
      {
        ushort num = this.RawReadUInt16();
        if (num == (ushort) 0)
          flag = true;
        else
          stringBuilder.Append((char) num);
      }
      return stringBuilder.ToString();
    }

    public string RetrieveString()
    {
      EGT.EGTReader.Entry entry = this.RetrieveEntry();
      return entry.Type == EGT.EGTReader.EntryType.String ? (string) entry.Value : throw new EGT.EGTReader.IOException(entry.Type, this.m_Reader);
    }

    public short RetrieveInt16()
    {
      EGT.EGTReader.Entry entry = this.RetrieveEntry();
      return entry.Type == EGT.EGTReader.EntryType.UInt16 ? (short) (ushort) entry.Value : throw new EGT.EGTReader.IOException(entry.Type, this.m_Reader);
    }

    public ushort RetrieveUInt16()
    {
      EGT.EGTReader.Entry entry = this.RetrieveEntry();
      return entry.Type == EGT.EGTReader.EntryType.UInt16 ? (ushort) entry.Value : throw new EGT.EGTReader.IOException(entry.Type, this.m_Reader);
    }

    public bool RetrieveBoolean()
    {
      EGT.EGTReader.Entry entry = this.RetrieveEntry();
      return entry.Type == EGT.EGTReader.EntryType.Boolean ? (bool) entry.Value : throw new EGT.EGTReader.IOException(entry.Type, this.m_Reader);
    }

    public byte RetrieveByte()
    {
      EGT.EGTReader.Entry entry = this.RetrieveEntry();
      return entry.Type == EGT.EGTReader.EntryType.Byte ? (byte) entry.Value : throw new EGT.EGTReader.IOException(entry.Type, this.m_Reader);
    }

    public bool GetNextRecord()
    {
      while ((int) this.m_EntriesRead < (int) this.m_EntryCount)
        this.RetrieveEntry();
      bool nextRecord;
      if (this.m_Reader.ReadByte() == (byte) 77)
      {
        this.m_EntryCount = this.RawReadUInt16();
        this.m_EntriesRead = (ushort) 0;
        nextRecord = true;
      }
      else
        nextRecord = false;
      return nextRecord;
    }

    public enum EntryType : byte
    {
      Error = 0,
      Boolean = 66, // 0x42
      Empty = 69, // 0x45
      UInt16 = 73, // 0x49
      String = 83, // 0x53
      Byte = 98, // 0x62
    }

    public class IOException : Exception
    {
      public IOException(string Message, Exception Inner)
        : base(Message, Inner)
      {
      }

      public IOException(EGT.EGTReader.EntryType Type, BinaryReader Reader)
        : base($"Type mismatch in file. Read '{((char) Type).ToString()}' at {(object) Reader.BaseStream.Position}")
      {
      }
    }

    public class Entry
    {
      public EGT.EGTReader.EntryType Type;
      public object Value;

      public Entry()
      {
        this.Type = EGT.EGTReader.EntryType.Empty;
        this.Value = (object) "";
      }

      public Entry(EGT.EGTReader.EntryType Type, object Value)
      {
        this.Type = Type;
        this.Value = Value;
      }
    }
  }
}
