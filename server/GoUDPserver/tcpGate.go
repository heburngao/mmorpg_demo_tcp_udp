package main

import (
	"net"
	dataMgr "./constant"
	//"fmt"
	"fmt"

	protoUtil "./protoc"
	msg "protocol/msg"

	"github.com/golang/protobuf/proto"

	"log"
	//"strconv"

	udp_ "./subserver"

	//"time"
	"time"
)
var PR = fmt.Println



var tcp_s *Server_tcp
var udp_s *udp_.Server_udp

//func GetServer() *Server_tcp{
//	//var s Server_tcp
//	//return s
//	return tcp_s
//}

func main() {
	udp_s = &udp_.Server_udp{}
	go udp_s.Start_UDP()

	tcp_s = &Server_tcp{}
	tcp_s.Start_TCP()

}
//var PT = fmt.Println
const(
	port_TCP string = ":42020"
	addr_TCP string = "0.0.0.0"
)

type Client_tcp struct{
	//index int
	playerInfo *msg.PlayerInfo
	userID string
	userName string
	userAddr *net.Addr
	conn *net.Conn
	c_msg chan []byte
}
//从chan读出，只能开支线程
func (c * Client_tcp) readOut(){
	for{
		//buff:= <- s.c_msg
		//s.analysis_cmd(buff)
		select {
		case buff:= <- c.c_msg:
			c.analysis_cmd(buff)
			//case <-time.After(3 * time.Second):
			//	log.Fatal("[TCP] time out ")
		default:
			//log.Fatal("[TCP] default")

		}

	}

}

//var num int
//var index = int(0)
func (c * Client_tcp) analysis_cmd (buff []byte)  {

	cmd := protoUtil.BytesToInt16(buff[0:2])//short
	pb := buff[2:]//[]byte
	PR("[TCP] analysis_cmd::########## 读取命令字:", cmd , " pb: " , pb , " addr : " , (*c.conn).RemoteAddr().String())
	switch cmd {
	case dataMgr.TCP_Enum_CreateSelf:
		pbObj := &msg.Rqst_CreateSelf{}
		err := proto.Unmarshal(pb, pbObj)
		if err != nil {
			log.Fatal("[ TCP ]  unmarshal create error: ", err)
			return
		}
		accountid := pbObj.Account
		password := pbObj.Password
		PR("[TCP] <<<<<<<<<<<<<<< account : " , *accountid , " password : " , *password)


		newclient := c //Client_tcp{}

		//acc := int(*accountid)
		acc2 := accountid//strconv.Itoa(acc)
		PR("uid: " , *acc2)
		nickname := "高贺兵"

		lever := int32(1)

		state := msg.State_IDLE

		//frameIndex := int32(0)

		//c.index += 10000
		//index1 := c.index % 30000
		//index2 := c.index / 30000
		//_X := int32(index1)
		//_Y := int32(0)
		//_Z := int32(index2 * 10000)

		/* optional
		_X := int32(0)
		_Y := int32(2 * 10000)
		_Z := int32(0)

		_Pos := msg.Vect3{
			X: &_X,
			Y: &_Y,
			Z: &_Z,
		}*/

		newclient.playerInfo = &msg.PlayerInfo{Userid:acc2 , Nickname:&nickname,Level:&lever, Status:&state }// ,Pos:&_Pos}
		//newclient.conn = c.conn xxx
		//newclient.userAddr = c.conn.RemoteAddr() xxx
		newclient.userID = *acc2
		newclient.userName = nickname

		//s.Clients[acc2] = newclient



		//=============================
		//	pl := newclient.playerInfo
		//	dataPB := &msg.Rspn_CreateSelf{
		//		Player:&pl,
		//	}
		cc := *c.conn
		fmt.Println("\n\r [TCP] ################## 下发生成角色 ################## uid : " , acc2 , "|||| ??  " ,cc.RemoteAddr().String())
		//
		//	pb_encodede, err := proto.Marshal(dataPB)
		//	if err != nil {
		//		log.Fatal("[TCP] marshaling error:", err)
		//	}
		//
		//	var encode_bytes []byte = make([]byte , 1024)
		//	encode_bytes = protoUtil.Packet(int16(dataMgr.TCP_Enum_CreateSelf),int16(0), pb_encodede)
		//======================= broadcast ======
		for _,v := range tcp_s.Clients {

			//=====
			//更新已生成玩家的坐标
			v.playerInfo.Pos , v.playerInfo.Dir = udp_s.GetPos(*v.playerInfo.Userid)
			//PR("[UDP]  得到 更新 坐标: " , new_pos)
			//=====
			PR("[TCP] >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 得到 更新 坐标 " ,v.playerInfo.Pos.String() ,  "uid: "  , *v.playerInfo.Userid)


			info := v.playerInfo
			//======
			dataPB := &msg.Rspn_CreateSelf{
				Player:info,
			}
			//fmt.Println("\n\r [TCP] ################## 下发生成角色 ################## uid : " , acc2 , "|||| " ,c.conn.RemoteAddr().String(), c.conn.RemoteAddr().String())
			pb_encodede, err := proto.Marshal(dataPB)
			if err != nil {
				log.Fatal("[TCP] born marshaling error:", err)
				return
			}
			var encode_bytes []byte = make([]byte , 1024)
			encode_bytes = protoUtil.Packet(int16(dataMgr.TCP_Enum_CreateSelf),int16(0), pb_encodede)
			//======
			for _,v := range tcp_s.Clients {
				_con := *v.conn
				n,err := _con.Write(encode_bytes)
				if err != nil {
					log.Fatal("[TCP] WriteTo born , error:", err)
					return
				}
				vv := *v.userAddr
				PR("[TCP] success~ write to newclient n: " , n ,"|" , encode_bytes ,  " newclient Addr: " , vv.String())
			}
			//=======
		}
	case dataMgr.TCP_Enum_HeartBeat:
		///PR("[TCP] HeartBeat cmd code: ",cmd, " pb: " , pb)
		pbObj := &msg.Rqst_HeartBeating{}
		err:= proto.Unmarshal(pb, pbObj)
		if err != nil {
			//dataMgr.CheckError(err)
			//log.Fatal("Heart Unamrshal : ",err) :::::: 注意: log.Fatal这玩意会杀死服务器进程
			return
		}
		//statusCode := pbObj.Status
		///PR("[TCP] HeartBeat status code : " , *statusCode)
		//TODO WriteToClient

		//players := []*msg.PlayerInfo{}
		var players []*msg.PlayerInfo
		for _,v := range tcp_s.Clients {
			players = append(players, v.playerInfo)
		}
		//=============================
		state := int32(1)
		dataPB := &msg.Rspn_HeartBeating{
			Status:&state,
			//PlayerList:players,//偏差拉回，在每次心跳返回时
		}

		fmt.Println("[TCP] ################## 下发心跳包 ##################" ,  c.conn  , c.userAddr)

		pb_encodede2, err := proto.Marshal(dataPB)
		if err != nil {
			log.Fatal("[TCP] heartbeating marshaling error:", err)
			return
		}

		var encode_bytes2 []byte = make([]byte , 1024)
		encode_bytes2 = protoUtil.Packet(int16(dataMgr.TCP_Enum_HeartBeat),int16(0), pb_encodede2)

		_con := *c.conn

		n,err := _con.Write(encode_bytes2)
		if err != nil {
			//log.Fatal("[TCP] heartbeating write ", err , " , ", _con)
			return
		}
		//for _,v := range s.Clients {
		//	n,err := v.conn.Write(encode_bytes2)
		//	if err != nil {
		//		dataMgr.LogError(err)
		//	}
		//	PR("[TCP] write to newclient n: " , n ,"|" , encode_bytes2 ,  " newclient Addr: " , v.userAddr.String())
		//
		//}
		PR("[TCP] write to newclient n: " , n ,"|" , encode_bytes2 ,  " newclient Addr: " , _con.RemoteAddr().String())
	default:
		PR("[TCP] unknown cmd code: ",cmd, " pb: " , pb)
	}



}

func (c * Client_tcp ) CastDestroy(){
	_con2 := *c.conn
	defer _con2.Close()
	//======================= broadcast ======
	//for _,v := range tcp_s.Clients {

	pl := append([]*msg.PlayerInfo{} , c.playerInfo)

	dataPB := &msg.Rspn_LeaveOffOthers{
		Player:pl,
	}
	//fmt.Println("\n\r [TCP] ################## 下发角色下线 ################## uid : " , c.playerInfo.Userid , "|||| ??? " ,_con2.RemoteAddr().String())
	fmt.Println("\n\r [TCP] ################## 下发角色下线 ################## ")
	pb_encodede, err := proto.Marshal(dataPB)
	if err != nil {
		//log.Fatal("[TCP] offline marshaling error:", err )
		return
	}

	var encode_bytes []byte = make([]byte , 1024)
	encode_bytes = protoUtil.Packet(int16(dataMgr.TCP_Enum_OthersLineOff),int16(0), pb_encodede)


	//告诉所有人
	for _,v := range tcp_s.Clients {
		_con := *v.conn
		n,err := _con.Write(encode_bytes)
		if err != nil {
			//log.Fatal("[TCP] WriteTo  offline error:", err)
			return
		}
		vv := *v.userAddr

		PR("[TCP] success~ write to newclient n: " , n ,"|" , encode_bytes ,  " newclient Addr: " , vv.String())
	}

	defer _con2.Close()

	//}
}
//type Msg_tcp struct{
//	status int
//	userID int
//	userName string
//	content []byte
//}
//=================



//########################################## server struct ##########################################

type Server_tcp struct{
	tmpBuf []byte
	Clients map [string] *Client_tcp
}


func (s * Server_tcp)Start_TCP(){
	PR("[TCP] init")
	tcp_addr, err := net.ResolveTCPAddr("tcp", port_TCP)
	PR("[TCP] address : ", tcp_addr)
	if err != nil {
		log.Fatal(" [TCP] addr err " , err)
		return
	}
	//dataMgr.CheckError(err)

	s.Clients = make(map[string] *Client_tcp, 0)

	listener, err := net.ListenTCP("tcp4", tcp_addr)
	defer listener.Close()
	//dataMgr.CheckError(err)
	if err != nil {
		log.Fatal(" [TCP] listner err " , err)
		return
	}
	PR("[TCP] NEW SERVER start")
	s.handleAccept(listener)

}






func (s * Server_tcp) handleAccept(listener *net.TCPListener){
	defer listener.Close()
	for {

		var err error
		var conn net.Conn
		conn,err = listener.Accept()
		if err != nil {

			continue
		}

		_client := &Client_tcp{}
		_client.conn = &conn
		_addr := conn.RemoteAddr()
		_client.userAddr = &_addr
		_client.c_msg = make(chan [] byte , 1024)
		s.Clients[_addr.String()] = _client

		PR("\r\n [TCP] xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx new Client come in xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx : ", conn.RemoteAddr().String() , " == " , _addr.String())

		go _client.readOut()


		go s.handleRead(conn) //主线程不可以，虽能用，但是只能以阻塞方式，建立一个lient link
	}
}




func (s * Server_tcp)handleRead(conn net.Conn)  {
	defer conn.Close()
	var buf [1024] byte
	for{
		//for _,_client := range s.Clients{

		n,err := conn.Read(buf[0:])
		if err != nil {
			PR("\n\r [TCP] disconnect !! " , conn.RemoteAddr().String())
			//dataMgr.CheckNetError(err,s.conn)
			for k,v := range s.Clients {
				_con := *v.conn
				if _con.RemoteAddr().String() == conn.RemoteAddr().String() {
					v.CastDestroy() //广播下线

					//_con.Close()//断开链接

					delete(s.Clients,k) //移出列表
					udp_s.RemoveClient(k) //移出列表
				}
			}
			///PR("[TCP] client n: " , len(s.Clients))
			return
		}
		buff := buf[0:n]
		PR("---> [TCP] rcv: " , n , buff , " addr: " , conn.RemoteAddr().String() , len(s.Clients))

		for k,v := range s.Clients {
			PR("k,v :: " , k , v.userAddr)
		}

		go s.CheckAlive(buff[0:],conn)

		//var connect protoUtil.Connect = protoUtil.Connect{s.conn, buff}
		if _client , ok := s.Clients[conn.RemoteAddr().String()] ; ok {

			PR("YES")
			s.tmpBuf = protoUtil.Unpack(append(s.tmpBuf,buff...),_client.c_msg)
		}else {
			PR("NO")
		}
		//PR("[TCP] analysis: ", string(s.tmpBuf) , s.tmpBuf )
		//}

	}

}
func (s *Server_tcp)CheckAlive(buffer []byte , conn net.Conn){
	var chn chan byte = make(chan byte)
	defer close(chn)
	defer conn.Close()
	go s.TimeOutCheck(chn , conn)
	for _,v := range buffer{
		chn <- v
	}
	//close(chn)
}
func (s *Server_tcp)TimeOutCheck(chn chan byte , conn net.Conn)  {

	select {
	case <- time.After(15*time.Second):
		//PR("[TCP] ######## time is out ")
		conn.Close()
	case <- chn :
		///PR("[TCP] ######## is alive ")
		conn.SetDeadline(time.Now().Add(20 * time.Second))
		break
	}

}