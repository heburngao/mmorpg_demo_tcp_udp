package subserver

import (
	"net"
	dataMgr "../constant"
	//"fmt"
	"time"
	//"fmt"

	protoUtil "../protoc"
	msg "protocol/msg"

	"github.com/golang/protobuf/proto"

	"log"
	//"strconv"
	//"container/list"

	"fmt"
)
//var PR = fmt.Println
//func main() {
//	start_UDP()
//}
var PR = fmt.Println
const(
	port_UDP string = ":52020"
	addr_UDP string = "0.0.0.0"
)
type Client_udp struct{
	uid string
	pos msg.Vect3
	dir msg.Vect3
	status_pb []byte //设计的有点多余，先观察一下
	userAddr *net.UDPAddr
	conn *net.UDPConn
	c_msg chan []byte
}

//从chan读出，只能开支线程
func (c * Client_udp) readOut(){
	for{
		buff:= <- c.c_msg
		c.analysis_cmd(buff)

		//select {
		//case buff:= <- c.c_msg:
		//	c.analysis_cmd(buff)
		//default:
		//	//log.Fatal("[TCP] default")
		//
		//}

	}

}

//var num int
//var index = int(0)
func (c * Client_udp) analysis_cmd (buff []byte)  {

	cmd := protoUtil.BytesToInt16(buff[0:2])//short
	pb := buff[2:]//[]byte
	//PR("[UDP] analysis_cmd::########## 读取命令字:", cmd , " len: " , len(pb), " pb: " , pb , " addr  is nil ??: " , c.conn.RemoteAddr())



	switch cmd {
		case dataMgr.UDP_Enum_UpdateStatus:
			pbObj := &msg.Rqst_UpdateStatus{} //值的指针
			err := proto.Unmarshal(pb, pbObj) // decode protobuff
			if err != nil {
				//log.Fatal("[ UDP ] a update status error:", err)
				return
			}
			//=====更新最新坐标，存入server内  只能用指针？？
				if c.uid != *pbObj.Info.Userid {
					c.uid = *pbObj.Info.Userid
				}
				c.pos = *pbObj.Info.Pos
				c.dir = *pbObj.Info.Dir
			PR("[UDP]  更新 坐标: " , *pbObj  ,"|||",  c.pos , "|||" , *c.dir.X,*c.dir.Y,*c.dir.Z)
			//=====更新最新坐标，存入server内
			udp_s.statusList = append(udp_s.statusList, pbObj.Info)


		default:
			PR("[UDP] unknown cmd code: ",cmd, " pb: " , pb)
	}


}

//xxxxxxxx




//########################################## server struct ##########################################
func (s * Server_udp ) GetPos(uid string) (*msg.Vect3 , *msg.Vect3){
	//这里面不能加打印，不然，服务器启动不了,干
	_X := int32(0)
	_Y := int32(1 * 10000)
	_Z := int32(0)

	new_pos := msg.Vect3{
		X: &_X,
		Y: &_Y,
		Z: &_Z,
	}
	dir_X := int32(-1)
	dir_Y := int32(-1)
	dir_Z := int32(-1)
	dir := msg.Vect3{
		X:&dir_X,
		Y:&dir_Y,
		Z:&dir_Z,
	}
	for _,v := range s.Clients{
		if v.uid == uid {
			new_pos = v.pos
			dir = v.dir
			return &new_pos , &dir
		}
	}

	return &new_pos , &dir
}
type Server_udp struct{
	tmpBuf     []byte
	Clients    map [string] *Client_udp
	statusList [] *msg.StatusInfo
}
var udp_s * Server_udp
func (s * Server_udp) Start_UDP() {
	PR("[UDP] init")
	udp_addr, err := net.ResolveUDPAddr("udp", port_UDP)
	PR("[UDP] address : ", udp_addr)
	dataMgr.CheckError(err)
	udp_s = s
	s.Clients = make(map[string] *Client_udp, 0)
	s.statusList = make([]*msg.StatusInfo, 0)
	s.tmpBuf = make([]byte,0)

	udpConn, err := net.ListenUDP("udp4", udp_addr)
	if err != nil {
		log.Fatal("[UDP] ",err)
		return
	}
	defer  udpConn.Close()

	PR("[UDP] NEW SERVER start")




	go s.Ticker() //轮循下发udp to clients

	s.handleAccept(udpConn)
}

//=================
//type Server_udp struct{
//	tmpBuf []byte
//	conn net.UDPConn
//	udp_addr net.UDPAddr
//
//	c_msg                 chan []byte
//	clients               map [string]Client_udp
//	update_status_clients map [string]msg.StatusInfo
//}
func (s *Server_udp)RemoveClient(uid string){
	if _,ok := s.Clients[uid]; ok {

		///PR("[UDP] A RemoveClient , clients_udp.count: " , len(s.clients))
		delete(s.Clients,uid)

	}
	///PR("[UDP] B RemoveClient , clients_udp.count: " , len(s.clients))
}



func (s * Server_udp) handleAccept(udpconn *net.UDPConn){
	var buffer []byte = make([]byte,478)
	//PR("[UDP] handleAccept local addr: " , udpconn.LocalAddr()  )
	for {
		//var n int
		//var err error
		//var conn *net.UDPConn
		n,addr,err := udpconn.ReadFromUDP(buffer[0:])


		if err != nil {
			continue
		}
		buff := buffer[0:n]
		//PR("\r\n [UDP] new Client come in xxxxxxxx : ", addr.String() , " n: " , n  , " 0.0.0.0:" , udpconn.LocalAddr() ,"|| nil: ", udpconn.RemoteAddr())
		PR("---> [UDP] rcv: " , n , buff)

		if client,ok := s.Clients[addr.String()]; ok {

			go client.readOut()
			go client.handleRead(buff)
		}else{

			_newclient := &Client_udp{}
			_newclient.conn = udpconn
			_newclient.userAddr = addr
			_newclient.c_msg = make(chan [] byte , 478)
			s.Clients[addr.String()] = _newclient


			go _newclient.readOut()
			go _newclient.handleRead(buff)
		}
	}
}
func (c * Client_udp)handleRead(buffer []byte)  {

		protoUtil.Unpack_UDP(buffer,c.c_msg)
}
func (s * Server_udp)Ticker(){
	timerTicker := time.Tick(time.Millisecond * 66)////每隔66毫秒执行一次把本轮收到的所有包列表下发
	for t := range timerTicker {
		var slist []*msg.StatusInfo = s.statusList//make([]*msg.StatusInfo,0)

		if 0 < len(slist) {

			//========
				PR("\r\n xxxxxxxxxxxxxxxxx len of slist : " , len(slist) )

			dataPB := &msg.Rspn_UpdateStatus{
				Info: slist,
			}

			pb_encodede, err := proto.Marshal(dataPB)
			if err != nil {
				//log.Fatal("[UDP] Ticker marshaling error:", err)
				continue
			}

			var encode_bytes []byte = make([]byte , 478)
			encode_bytes = protoUtil.Packet(int16(dataMgr.UDP_Enum_UpdateStatus),int16(0), pb_encodede)
			//========
			for _,v := range s.Clients {
				n , err := v.conn.WriteToUDP(encode_bytes,v.userAddr)
				if err != nil {
					log.Fatal("[UDP] WriteTo error:", err)
					return
				}
				PR("[UDP] success~ write to client n: " , n ,"| slist.len: " ,len(slist) , "|" , encode_bytes ,  " client Addr: " , v.userAddr.String())
			}
			//========
			s.statusList = make([] *msg.StatusInfo,0)
		}

		t.Clock()
	}
}






