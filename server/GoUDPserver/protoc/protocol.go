package protoc

import (
	"bytes"
	"encoding/binary"
	"fmt"
	"net"
)
// DB + LEN + CMD  + PAYLOAD  // short + int + short + []byte
const (
	HeadWords = "DB"
	HeaderLen = 2 //short
	BufferLen = 4 //int
	CMDLen = 2
	RETLen = 2
)
type Connect struct {
	M_conn net.Conn
	M_msg  []byte
}
var PR = fmt.Println
//封包
func Packet(cmd int16 , ret int16, message []byte) []byte {
	//DB + TOTOAL_LEN + CMD + RET + PAYLOAD     01  2345  67  89  10:n
	///PR("Packet A: cmd :" ,cmd ,":",Int16ToBytes(cmd), " ret: ", ret ,":" ,Int16ToBytes(ret), " msg: " , message)
	totoallen := HeaderLen+BufferLen+CMDLen+RETLen+ len(message)
	var buffer []byte //= make([]byte ,0,1024)
	buffer = append(buffer ,[]byte(HeadWords)...) // DB  short
	buffer = append(buffer ,IntToBytes(totoallen)...) // LEN  int
	buffer = append(buffer ,Int16ToBytes(cmd)...) //CMD short
	buffer = append(buffer ,Int16ToBytes(ret)...) // RET  short
	buffer = append(buffer ,message...)
	///PR("Packet B: " , buffer)
	return buffer
	//return append(append(append(append([]byte(HeadWords), IntToBytes(totoallen)...), Int16ToBytes(cmd)...), Int16ToBytes(ret)...), message...)
}

//func Unpack2(buffer_Connect []byte, readerChannel chan Connect , conn net.Conn) Connect {
//	len_rcv := len(buffer)
//
//	var i int
//	for i = 0; i < len_rcv; i = i + 1 {
//		if len_rcv < i+HeaderLen+BufferLen {
//			PR("[TCP] break len_rcv: " , len_rcv)
//			break
//		}
//		if string(buffer[i:i+HeaderLen]) == HeadWords {//识别DB words
//			//messagelength : BufferLen 长度对应的内容表达长度
//			//[68 66 0 0 0 15 39 16 8 189 8 16 192 196 7]  = messageLength = buffer[i+HeaderLen : i+HeaderLen+BufferLen] = [0,0,0,15]
//			totoal_msg_len := BytesToInt32(buffer[i+HeaderLen : i+HeaderLen+BufferLen])
//			//if len_rcv < i+HeaderLen+BufferLen+messageLength {
//			if len_rcv < i+ totoal_msg_len {
//				PR("[TCP] break b " , len_rcv, totoal_msg_len , i, HeaderLen , BufferLen)
//				break
//			}
//			//PR("[TCP] x" , buffer[i+HeaderLen+BufferLen : i+messageLength])
//			//data := buffer[i+HeaderLen+BufferLen : i+HeaderLen+BufferLen+messageLength]
//			protobuff := buffer[i+HeaderLen+BufferLen : i+totoal_msg_len]
//			///PR("[TCP] buffer(cmd+payload): " , protobuff , " index: " , i)
//			readerChannel <- Connect{conn,protobuff}
//
//			//i =  i + HeaderLen + BufferLen + messageLength - 1
//			i = i + totoal_msg_len - 1
//			//PR("[TCP] cc " , data , " index: " , i)
//		}
//	}
//
//	if i == len_rcv {
//		///PR("[TCP] return Unpack finished " , i)
//		//return make([]byte, 0)
//		return Connect{conn,make([]byte,0)}
//	}
//	///PR("[TCP] xxxxxx return buffer[i:] : " , buffer[i:])
//	//return buffer[i:]
//	return Connect{conn,buffer[i:]}
//}

func Unpack_UDP(buffer []byte, cmd_payload chan []byte) {
	len_rcv := len(buffer)

	i := 0
	if len_rcv < i+HeaderLen+BufferLen {
		PR("[UDP] break len_rcv: " , len_rcv , "<" , i+HeaderLen+BufferLen , " i: " , i )
		return
	}
	if string(buffer[i:i+HeaderLen]) == HeadWords { //识别DB words
		//messagelength : BufferLen 长度对应的内容表达长度
		//[68 66 0 0 0 15 39 16 8 189 8 16 192 196 7]  = messageLength = buffer[i+HeaderLen : i+HeaderLen+BufferLen] = [0,0,0,15]
		totoal_msg_len := BytesToInt32(buffer[i+HeaderLen: i+HeaderLen+BufferLen])
		if len_rcv < i+totoal_msg_len {
			PR("[UDP] break b ", len_rcv, totoal_msg_len, i, HeaderLen, BufferLen)
			return
		}
		cmd_protobuff := buffer[i+HeaderLen+BufferLen: i+totoal_msg_len]
		cmd_payload <- cmd_protobuff
	}
}
//解包
func Unpack(buffer []byte, readerChannel chan []byte) []byte {
	len_rcv := len(buffer)

	var i int
	for i = 0; i < len_rcv; i = i + 1 {
		if len_rcv < i+HeaderLen+BufferLen {
			PR("[TCP] break len_rcv: " , len_rcv)
			break
		}
		if string(buffer[i:i+HeaderLen]) == HeadWords {//识别DB words
			//messagelength : BufferLen 长度对应的内容表达长度
			//[68 66 0 0 0 15 39 16 8 189 8 16 192 196 7]  = messageLength = buffer[i+HeaderLen : i+HeaderLen+BufferLen] = [0,0,0,15]
			totoal_msg_len := BytesToInt32(buffer[i+HeaderLen : i+HeaderLen+BufferLen])
			//if len_rcv < i+HeaderLen+BufferLen+messageLength {
			if len_rcv < i+ totoal_msg_len {
				PR("[TCP] break b " , len_rcv, totoal_msg_len , i, HeaderLen , BufferLen)
				break
			}
			//PR("[TCP] x" , buffer[i+HeaderLen+BufferLen : i+messageLength])
			//data := buffer[i+HeaderLen+BufferLen : i+HeaderLen+BufferLen+messageLength]
			protobuff := buffer[i+HeaderLen+BufferLen : i+totoal_msg_len]
			///PR("[TCP] buffer(cmd+payload): " , protobuff , " index: " , i)
			readerChannel <- protobuff

			//i =  i + HeaderLen + BufferLen + messageLength - 1
			i = i + totoal_msg_len - 1
			//PR("[TCP] cc " , data , " index: " , i)
		}
	}

	if i == len_rcv {
		///PR("[TCP] return Unpack finished " , i)
		return buffer[0:0]//make([]byte, 0)
	}
	///PR("[TCP] xxxxxx return buffer[i:] : " , buffer[i:])
	return buffer[i:]
}

//整形转换成字节
func IntToBytes(n int) []byte {
	x := int32(n)

	bytesBuffer := bytes.NewBuffer([]byte{})
	binary.Write(bytesBuffer, binary.BigEndian, x)
	return bytesBuffer.Bytes()
}
func Int16ToBytes(n int16) []byte {
	x := int16(n)

	bytesBuffer := bytes.NewBuffer([]byte{})
	binary.Write(bytesBuffer, binary.BigEndian, x)
	return bytesBuffer.Bytes()
}
func Int8ToBytes(n int8) []byte {
	x := int8(n)

	bytesBuffer := bytes.NewBuffer([]byte{})
	binary.Write(bytesBuffer, binary.BigEndian, x)
	return bytesBuffer.Bytes()
}
//=============================
//字节转换成整形
func BytesToInt32(b []byte) int {
	bytesBuffer := bytes.NewBuffer(b)

	var x int32
	binary.Read(bytesBuffer, binary.BigEndian, &x)

	return int(x)
}


func BytesToInt16(b []byte) int16 {
	bytesBuffer := bytes.NewBuffer(b)

	var x int16
	binary.Read(bytesBuffer, binary.BigEndian, &x)

	return int16(x)
}

func BytesToInt8(b []byte) int8 {
	bytesBuffer := bytes.NewBuffer(b)

	var x int8
	binary.Read(bytesBuffer, binary.BigEndian, &x)

	return int8(x)
}
