// Code generated by protoc-gen-go. DO NOT EDIT.
// source: Rspn_StatusUpdateMove.proto

package msg

import proto "github.com/golang/protobuf/proto"
import fmt "fmt"
import math "math"

// Reference imports to suppress errors if they are not otherwise used.
var _ = proto.Marshal
var _ = fmt.Errorf
var _ = math.Inf

type Rspn_StatusUpdateMove struct {
	Player           []*PlayerInfo `protobuf:"bytes,1,rep,name=Player" json:"Player,omitempty"`
	XXX_unrecognized []byte        `json:"-"`
}

func (m *Rspn_StatusUpdateMove) Reset()                    { *m = Rspn_StatusUpdateMove{} }
func (m *Rspn_StatusUpdateMove) String() string            { return proto.CompactTextString(m) }
func (*Rspn_StatusUpdateMove) ProtoMessage()               {}
func (*Rspn_StatusUpdateMove) Descriptor() ([]byte, []int) { return fileDescriptor12, []int{0} }

func (m *Rspn_StatusUpdateMove) GetPlayer() []*PlayerInfo {
	if m != nil {
		return m.Player
	}
	return nil
}

func init() {
	proto.RegisterType((*Rspn_StatusUpdateMove)(nil), "msg.Rspn_StatusUpdateMove")
}

func init() { proto.RegisterFile("Rspn_StatusUpdateMove.proto", fileDescriptor12) }

var fileDescriptor12 = []byte{
	// 99 bytes of a gzipped FileDescriptorProto
	0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0xff, 0xe2, 0x92, 0x0e, 0x2a, 0x2e, 0xc8,
	0x8b, 0x0f, 0x2e, 0x49, 0x2c, 0x29, 0x2d, 0x0e, 0x2d, 0x48, 0x49, 0x2c, 0x49, 0xf5, 0xcd, 0x2f,
	0x4b, 0xd5, 0x2b, 0x28, 0xca, 0x2f, 0xc9, 0x17, 0x62, 0xce, 0x2d, 0x4e, 0x97, 0x12, 0x08, 0xc8,
	0x49, 0xac, 0x4c, 0x2d, 0xf2, 0xcc, 0x4b, 0xcb, 0x87, 0x08, 0x2b, 0x59, 0x70, 0x89, 0x62, 0xd5,
	0x25, 0x24, 0xcf, 0xc5, 0x06, 0x51, 0x2c, 0xc1, 0xa8, 0xc0, 0xac, 0xc1, 0x6d, 0xc4, 0xaf, 0x97,
	0x5b, 0x9c, 0xae, 0x87, 0xd0, 0x0f, 0x08, 0x00, 0x00, 0xff, 0xff, 0x12, 0xbc, 0x48, 0x62, 0x6e,
	0x00, 0x00, 0x00,
}