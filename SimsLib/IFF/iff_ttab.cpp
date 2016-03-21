#include "iff_ttab.h"
#include "crack.h"
#include <iostream>
using std::cout;
using std::endl;

// this is the base class for scanning a TTAB entry.  It maps fields
// into the correct input stream operation so that the parser need
// only concern itself with the mechanics of putting stuff in the
// right places and not how the values are represented in the input.
class ScanTTAB
{
protected:
	crack &m_in;			// access to input stream
public:
	ScanTTAB(crack &in) : m_in(in) {}
	// stream reader functions
	virtual int readShort(void)	{ return m_in.readShort(); }
	virtual int readInt(void)	{ return m_in.readInt(); }
	virtual float readFloat(void)	{ return m_in.readFloat(); }
	void setFields(char *wid)	{ m_in.setFieldWidths(wid); }
	// variations in field format
	// This number was originally a 2-byte integer and later
	// expanded to four bytes.
	virtual int read16(void)	{ return readShort(); }
	// The flags replaced a 2-byte field that was originally
	// skipped, then later expanded to four bytes.
	virtual int readFlags(void)	{ readShort(); return 0; }
	// Earlier versions do not have the index into the associated
	// string table and menus were one-to-one with the strings.  If
	// we're reading a format without an index, readMenu returns
	// the index passed in.
	virtual int readMenu(int i)	{ return i; }
	// Attenuation values were always expressed as a custom float
	// value.  Later a code was added to provide certain standard
	// values (low, moderate, high).
	virtual int readAtten(void)	{ return 0; }
	// Interactions were intially always available.  Later, certain
	// actions were available only if the user initiated them.
	virtual int readAuto(void)	{ return 0; }
	// "join group"
	virtual int readJoin(void)	{ return -1; }
	// Initially, motives only advertised the delta value.  Later,
	// the threshhold and character type were added.
	virtual int readMotive(void)	{ return 0; }
	// This field appears in TSO; I have no idea what it does
	virtual int readWTF(void)	{ return 0; }
	// When all else fails, dump out the resource so we can pick
	// it apart more easily.
	void dump(void) {
		m_in.dump(512, crack::dumpASCII | crack::dumpBinary
			| crack::dumpShort | crack::dumpLittle);
	}
};

// each class for a specific input version overrides the functions for
// those fields whose layout is different, either by changing size or
// by coming into existance
class ScanTTAB2 : public ScanTTAB // earliest version for which samples exist
{
public:
	ScanTTAB2(crack &in) : ScanTTAB(in) {}
	// just use the base class functions
};
class ScanTTAB3 : public ScanTTAB2
{
public:
	ScanTTAB3(crack &in) : ScanTTAB2(in) {}
	// added flag field (was skipped)
	int readFlags(void)	{ return read16(); }
};
// no samples of TTAB v.4, so we don't know what changed there
class ScanTTAB5 : public ScanTTAB3
{
public:
	ScanTTAB5(crack &in) : ScanTTAB3(in) {}
	// flag fields become a full int
	int read16(void)	{ return readInt(); }
	// new fields
	int readMenu(int i)	{ return readInt(); }
	int readAuto(void)	{ return readInt(); }
	int readJoin(void)	{ return readInt(); }
};
// no (non-zero) samples of TTAB v.6, so we don't know what changed there
class ScanTTAB7 : public ScanTTAB5
{
public:
	ScanTTAB7(crack &in) : ScanTTAB5(in) {}
	// new fields
	int readAtten(void)	{ return read16(); }
	int readMotive(void)	{ return readShort(); }
};
class ScanTTAB8 : public ScanTTAB7
{
public:
	ScanTTAB8(crack &in) : ScanTTAB7(in) {}
	// seemingly unchanged...
};
class ScanTTAB9 : public ScanTTAB8
{
public:
	ScanTTAB9(crack &in) : ScanTTAB8(in) {}
	// field encoding
	int readShort(void)	{ return m_in.getField(); }
	int readInt(void)	{ return m_in.getField(); }
	float readFloat(void)	{ return m_in.getFloat(); }
};
class ScanTTAB10 : public ScanTTAB9
{
public:
	ScanTTAB10(crack &in) : ScanTTAB9(in) {}
	// WTF?
	int readWTF(void)	{ return m_in.getField(); }
};
// this is the same shape as v.10, but it's uncompressed again,
// so we derive from v.8 and re-apply the field change
class ScanTTAB11 : public ScanTTAB8
{
public:
	ScanTTAB11(crack &in) : ScanTTAB8(in) {}
	// WTF?
	int readWTF(void)	{ return readInt(); }
};

static inline int compressionByte(crack &crak)
{
	if (crak.readByte() != 1) {
#ifdef DebugPrint
		cout << "LOOK: compression not one" << endl;// DEBUG
#endif
		return 1;
	}
	return 0;
}
simIFF_TTAB::simIFF_TTAB(std::istream *s)
:	m_interact(0)
{
	crackLittle crak(s);
	if (crak.isEOF()) return;	// empty (strange, but it happens)
	m_count = crak.readShort();
	if (m_count <= 0) {
#ifdef DebugPrint
	cout << "LOOK: illegal count " << m_count << endl;	// DEBUG
#endif
		return;
	}
	if (crak.isEOF()) return;	// too short (strange, but it happens)
	int encoding = crak.readShort();
	ScanTTAB *in;
	if (encoding == 9) {
		if (compressionByte(crak)) { m_count = -1; return; }
		in = new ScanTTAB9(crak);
	} else if (encoding == 10) {
		if (compressionByte(crak)) { m_count = -1; return; }
		in = new ScanTTAB10(crak);
	} else
	if (encoding == 8) in = new ScanTTAB8(crak); else
	if (encoding == 11) in = new ScanTTAB11(crak); else
	if (encoding == 7) in = new ScanTTAB7(crak); else
	// one type 6 with no menus
	if (encoding == 5) in = new ScanTTAB5(crak); else
	// no type 4 found
	if (encoding == 3) in = new ScanTTAB3(crak); else
	if (encoding == 2) in = new ScanTTAB2(crak); else
	{
		cout << "LOOK: can't handle TTAB version " << encoding << endl;
		m_count = -1;
		crak.dump();
		return;
	}
#ifdef DebugPrint
	cout << "Decoding version " << encoding << endl;	// DEBUG
#endif
	m_interact = new Interact[m_count];
	static char widShort[4] = { 5, 8, 13, 16 };
	static char widLong[4] = { 6, 11, 21, 32 };
	for (int i = 0; i < m_count; ++i) {
		in->setFields(widShort);
		Interact &x = m_interact[i];
		x.m_action = in->readShort();
		x.m_guard = in->readShort();
		in->setFields(widLong);
		int sixteen = in->read16();	// always 16
		if (sixteen != 16) {
#ifdef DebugPrint
			cout << "LOOK: sixteen != " << sixteen << endl;
#endif
			delete in;
			m_count = -1;
			return;
		}
		x.m_flags = in->readFlags();
		x.m_stringx = in->readMenu(i);
		x.m_attenuation = in->readAtten(); // 0..4
		x.m_custom = in->readFloat();
		x.m_autonomy = in->readAuto();
		x.m_join = in->readJoin();	// "index for joining"?
		in->setFields(widShort);
		for (int j = 0; j < sixteen; ++j) {
			x.m_motive[j].limit = in->readMotive();
			x.m_motive[j].delta = in->readShort();
			x.m_motive[j].type = in->readMotive();
		}
		in->setFields(widLong);
		x.m_WTF = in->readWTF();
	}
	// in->dump();
	delete in;
}

simIFF_TTAB::~simIFF_TTAB(void)
{
	delete[] m_interact;
}
