// Code generated by protoc-gen-go. DO NOT EDIT.
// source: PlayerInfo.proto

package msg

import proto "github.com/golang/protobuf/proto"
import fmt "fmt"
import math "math"

// Reference imports to suppress errors if they are not otherwise used.
var _ = proto.Marshal
var _ = fmt.Errorf
var _ = math.Inf

type State int32

const (
	State_NONE      State = 0
	State_IDLE      State = 1
	State_WALK      State = 2
	State_RUN       State = 3
	State_JUMP      State = 4
	State_FLY       State = 5
	State_SWIM      State = 7
	State_ATTACK    State = 8
	State_HURT      State = 9
	State_SKILL     State = 10
	State_DIE       State = 11
	State_LINEOFF   State = 12
	State_SPRINT    State = 13
	State_ROTATE    State = 14
	State_SWIM_IDLE State = 15
)

var State_name = map[int32]string{
	0:  "NONE",
	1:  "IDLE",
	2:  "WALK",
	3:  "RUN",
	4:  "JUMP",
	5:  "FLY",
	7:  "SWIM",
	8:  "ATTACK",
	9:  "HURT",
	10: "SKILL",
	11: "DIE",
	12: "LINEOFF",
	13: "SPRINT",
	14: "ROTATE",
	15: "SWIM_IDLE",
}
var State_value = map[string]int32{
	"NONE":      0,
	"IDLE":      1,
	"WALK":      2,
	"RUN":       3,
	"JUMP":      4,
	"FLY":       5,
	"SWIM":      7,
	"ATTACK":    8,
	"HURT":      9,
	"SKILL":     10,
	"DIE":       11,
	"LINEOFF":   12,
	"SPRINT":    13,
	"ROTATE":    14,
	"SWIM_IDLE": 15,
}

func (x State) Enum() *State {
	p := new(State)
	*p = x
	return p
}
func (x State) String() string {
	return proto.EnumName(State_name, int32(x))
}
func (x *State) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(State_value, data, "State")
	if err != nil {
		return err
	}
	*x = State(value)
	return nil
}
func (State) EnumDescriptor() ([]byte, []int) { return fileDescriptor1, []int{0} }

type StatusInfo struct {
	Userid           *string `protobuf:"bytes,1,req,name=Userid" json:"Userid,omitempty"`
	Pos              *Vect3  `protobuf:"bytes,2,opt,name=Pos" json:"Pos,omitempty"`
	Status           *State  `protobuf:"varint,3,opt,name=Status,enum=msg.State,def=0" json:"Status,omitempty"`
	MoveSpeed        *int32  `protobuf:"varint,4,opt,name=MoveSpeed" json:"MoveSpeed,omitempty"`
	Dir              *Vect3  `protobuf:"bytes,5,opt,name=Dir" json:"Dir,omitempty"`
	XXX_unrecognized []byte  `json:"-"`
}

func (m *StatusInfo) Reset()                    { *m = StatusInfo{} }
func (m *StatusInfo) String() string            { return proto.CompactTextString(m) }
func (*StatusInfo) ProtoMessage()               {}
func (*StatusInfo) Descriptor() ([]byte, []int) { return fileDescriptor1, []int{0} }

const Default_StatusInfo_Status State = State_NONE

func (m *StatusInfo) GetUserid() string {
	if m != nil && m.Userid != nil {
		return *m.Userid
	}
	return ""
}

func (m *StatusInfo) GetPos() *Vect3 {
	if m != nil {
		return m.Pos
	}
	return nil
}

func (m *StatusInfo) GetStatus() State {
	if m != nil && m.Status != nil {
		return *m.Status
	}
	return Default_StatusInfo_Status
}

func (m *StatusInfo) GetMoveSpeed() int32 {
	if m != nil && m.MoveSpeed != nil {
		return *m.MoveSpeed
	}
	return 0
}

func (m *StatusInfo) GetDir() *Vect3 {
	if m != nil {
		return m.Dir
	}
	return nil
}

type PlayerInfo struct {
	Userid           *string `protobuf:"bytes,1,req,name=Userid" json:"Userid,omitempty"`
	Nickname         *string `protobuf:"bytes,2,opt,name=Nickname" json:"Nickname,omitempty"`
	Pos              *Vect3  `protobuf:"bytes,3,opt,name=Pos" json:"Pos,omitempty"`
	Level            *int32  `protobuf:"varint,4,opt,name=Level" json:"Level,omitempty"`
	Status           *State  `protobuf:"varint,5,opt,name=Status,enum=msg.State,def=0" json:"Status,omitempty"`
	Dir              *Vect3  `protobuf:"bytes,6,opt,name=Dir" json:"Dir,omitempty"`
	XXX_unrecognized []byte  `json:"-"`
}

func (m *PlayerInfo) Reset()                    { *m = PlayerInfo{} }
func (m *PlayerInfo) String() string            { return proto.CompactTextString(m) }
func (*PlayerInfo) ProtoMessage()               {}
func (*PlayerInfo) Descriptor() ([]byte, []int) { return fileDescriptor1, []int{1} }

const Default_PlayerInfo_Status State = State_NONE

func (m *PlayerInfo) GetUserid() string {
	if m != nil && m.Userid != nil {
		return *m.Userid
	}
	return ""
}

func (m *PlayerInfo) GetNickname() string {
	if m != nil && m.Nickname != nil {
		return *m.Nickname
	}
	return ""
}

func (m *PlayerInfo) GetPos() *Vect3 {
	if m != nil {
		return m.Pos
	}
	return nil
}

func (m *PlayerInfo) GetLevel() int32 {
	if m != nil && m.Level != nil {
		return *m.Level
	}
	return 0
}

func (m *PlayerInfo) GetStatus() State {
	if m != nil && m.Status != nil {
		return *m.Status
	}
	return Default_PlayerInfo_Status
}

func (m *PlayerInfo) GetDir() *Vect3 {
	if m != nil {
		return m.Dir
	}
	return nil
}

// message Vect2 {
// 	required float X = 1;
// 	required float Y = 2;
// }
type Vect3 struct {
	X                *int32 `protobuf:"varint,1,req,name=X" json:"X,omitempty"`
	Y                *int32 `protobuf:"varint,2,req,name=Y" json:"Y,omitempty"`
	Z                *int32 `protobuf:"varint,3,req,name=Z" json:"Z,omitempty"`
	XXX_unrecognized []byte `json:"-"`
}

func (m *Vect3) Reset()                    { *m = Vect3{} }
func (m *Vect3) String() string            { return proto.CompactTextString(m) }
func (*Vect3) ProtoMessage()               {}
func (*Vect3) Descriptor() ([]byte, []int) { return fileDescriptor1, []int{2} }

func (m *Vect3) GetX() int32 {
	if m != nil && m.X != nil {
		return *m.X
	}
	return 0
}

func (m *Vect3) GetY() int32 {
	if m != nil && m.Y != nil {
		return *m.Y
	}
	return 0
}

func (m *Vect3) GetZ() int32 {
	if m != nil && m.Z != nil {
		return *m.Z
	}
	return 0
}

func init() {
	proto.RegisterType((*StatusInfo)(nil), "msg.StatusInfo")
	proto.RegisterType((*PlayerInfo)(nil), "msg.PlayerInfo")
	proto.RegisterType((*Vect3)(nil), "msg.Vect3")
	proto.RegisterEnum("msg.State", State_name, State_value)
}

func init() { proto.RegisterFile("PlayerInfo.proto", fileDescriptor1) }

var fileDescriptor1 = []byte{
	// 343 bytes of a gzipped FileDescriptorProto
	0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0xff, 0x74, 0x8e, 0xcf, 0x4e, 0xea, 0x40,
	0x14, 0x87, 0xef, 0xb4, 0x4c, 0xa1, 0x87, 0x0b, 0xf7, 0xdc, 0xd9, 0xd8, 0x65, 0xc3, 0xaa, 0x71,
	0xc1, 0x42, 0x77, 0xee, 0x1a, 0x29, 0xb1, 0x32, 0x94, 0xa6, 0x7f, 0x44, 0xdc, 0x18, 0x02, 0x23,
	0x21, 0x02, 0x25, 0x6d, 0x25, 0x71, 0xed, 0x13, 0xf8, 0x18, 0xbe, 0xa5, 0x39, 0x05, 0x25, 0x91,
	0xb8, 0xfb, 0xf2, 0x9b, 0x9c, 0x6f, 0x3e, 0xc0, 0x70, 0x35, 0x7d, 0x55, 0xb9, 0xbf, 0x79, 0xca,
	0xba, 0xdb, 0x3c, 0x2b, 0x33, 0xa1, 0xaf, 0x8b, 0x45, 0xe7, 0x8d, 0x01, 0xc4, 0xe5, 0xb4, 0x7c,
	0x29, 0xe8, 0x45, 0xb4, 0xc1, 0x48, 0x0b, 0x95, 0x2f, 0xe7, 0x16, 0xb3, 0x35, 0xc7, 0x14, 0x67,
	0xa0, 0x87, 0x59, 0x61, 0x69, 0x36, 0x73, 0x9a, 0x17, 0xd0, 0x5d, 0x17, 0x8b, 0xee, 0x9d, 0x9a,
	0x95, 0x97, 0xc2, 0x06, 0x63, 0x7f, 0x66, 0xe9, 0x36, 0x73, 0xda, 0x87, 0x37, 0x9a, 0xd4, 0x55,
	0x2d, 0x18, 0x05, 0x9e, 0xf8, 0x0f, 0xe6, 0x30, 0xdb, 0xa9, 0x78, 0xab, 0xd4, 0xdc, 0xaa, 0xd9,
	0xcc, 0xe1, 0x64, 0xeb, 0x2d, 0x73, 0x8b, 0xff, 0xb4, 0x75, 0xde, 0x19, 0xc0, 0xb1, 0xef, 0xa4,
	0x02, 0xa1, 0x11, 0x2c, 0x67, 0xcf, 0x9b, 0xe9, 0x5a, 0x55, 0x29, 0xdf, 0x5d, 0xfa, 0x49, 0x57,
	0x0b, 0xb8, 0x54, 0x3b, 0xb5, 0x3a, 0xfc, 0x78, 0xcc, 0xe4, 0xbf, 0x64, 0x1e, 0x9a, 0x8c, 0x93,
	0x26, 0x07, 0xf8, 0x5e, 0x69, 0x02, 0xbb, 0xaf, 0x42, 0x38, 0xe1, 0xc4, 0xd2, 0xbe, 0xf0, 0xc1,
	0xd2, 0x09, 0xcf, 0x3f, 0x18, 0xf0, 0x4a, 0x29, 0x1a, 0x50, 0x49, 0xf1, 0x0f, 0x91, 0xdf, 0x93,
	0x1e, 0x32, 0xa2, 0xb1, 0x2b, 0x07, 0xa8, 0x89, 0x3a, 0xe8, 0x51, 0x1a, 0xa0, 0x4e, 0xd3, 0x6d,
	0x3a, 0x0c, 0xb1, 0x46, 0x53, 0x5f, 0x4e, 0x90, 0xd3, 0x14, 0x8f, 0xfd, 0x21, 0xd6, 0x05, 0x80,
	0xe1, 0x26, 0x89, 0x7b, 0x3d, 0xc0, 0x06, 0xad, 0x37, 0x69, 0x94, 0xa0, 0x29, 0x4c, 0xe0, 0xf1,
	0xc0, 0x97, 0x12, 0x81, 0x6e, 0x7a, 0xbe, 0x87, 0x4d, 0xd1, 0x84, 0xba, 0xf4, 0x03, 0x6f, 0xd4,
	0xef, 0xe3, 0x5f, 0x3a, 0x8b, 0xc3, 0xc8, 0x0f, 0x12, 0x6c, 0x11, 0x47, 0xa3, 0xc4, 0x4d, 0x3c,
	0x6c, 0x8b, 0x16, 0x98, 0x24, 0x7e, 0xac, 0x6a, 0xfe, 0x7d, 0x06, 0x00, 0x00, 0xff, 0xff, 0x1a,
	0x78, 0xc3, 0xba, 0x07, 0x02, 0x00, 0x00,
}