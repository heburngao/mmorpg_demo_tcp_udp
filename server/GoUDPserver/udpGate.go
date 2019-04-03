//package main
//
//import (
//	"fmt"
//	"net"
//	"os"
//	"time"
//	"sync"
//
//	//"./protocol"
//
//)
//
//const(
//	//port_UDP string = ":52020"
//	addr string = "0.0.0.0"
//)
//type Client struct{
//	userID int
//	userName string
//	userAddr *net.UDPAddr
//
//}
//
//type Msg struct{
//	status int
//	userID int
//	userName string
//	content []byte
//}
////=================
//type Server struct{
//	conn *net.UDPConn
//	c_msg chan []byte
//	clients map [int]Client
//}
//
//
//
////func (s * Server) handlerMsg(){
////	var buf [1024]byte
////	n,addr , err := s.conn.ReadFromUDP(buf[0:])
////	if err != nil {
////		return
////	}
////
////	buff := buf[0:n]
////
////	var m Msg = s.analayzeMsg(buff)
////	switch m.status {
////	case 1:
////
////		var c Client
////		c.userAddr = addr
////		c.userID = m.userID
////		c.userName = m.userName
////
////		s.clients[c.userID] = c
////		s.c_msg  <- buff
////	case 2:
////		s.c_msg <- buff
////	case 3:
////		delete(s.clients, m.userID)
////		s.c_msg <- buff
////	default:
////		PT("unknow error:", buff)
////	}
////}
//
//
//func (s * Server) readOut(){
//
//	for{
//		buff := <- s.c_msg
//		daytime := time.Now().String()
//
//		PT(string(buff), daytime)
//		// send back to the clients
//	/*	sendstr := buff + daytime
//		for _, c := range s.clients {
//			n, err := s.conn.WriteToUDP([]byte(sendstr) , c.userAddr)
//		}
//	*/
//	}
//}
//
//var PT = fmt.Println
//
//
////=======================================================================================
//func main() {
//
//	goUDP()
//}
//
////###### udp #########
//func goUDP() {
//	udp_addr, err := net.ResolveUDPAddr("udp4", port_UDP)
//	PR("[UDP] address : ", udp_addr)
//	dataMgr.CheckError(err)
//	PR("[UDP] recv:1")
//
///*
//	addr := net.UDPAddr{
//		Port: 2000,
//		IP: net.ParseIP("127.0.0.1"),
//	}
//	ser, err := net.ListenUDP("udp", &addr)
//*/
//	//conn,err := net.Listen("udp",addr+":2020")
//
//
//	var s Server
//	s.c_msg = make(chan []byte,1024)
//	s.clients = make(map[int] Client, 0)
//
//	s.conn, err = net.ListenUDP("udp", udp_addr)
//
//	fmt.Println("[ UDP ] recv:2")
//	defer s.conn.Close()
//	fmt.Println("[ UDP ] recv:3")
//	dataMgr.CheckError(err)
//
//	fmt.Println("[ UDP ] recv:4 " )
//
//	go s.readOut()
//
//	for{
//		s.handlerMsg()
//	}
//
//	////////////////以前的处理方式 ： go recvUDPmsg(s.conn)
//	fmt.Println("[ UDP ] recv:5")
//}
//
//
//
////########/###### udp ########
//
//func recvUDPmsg(connUDP *net.UDPConn) {
//	if len(dataMgr.Clients_UDP) >= 200 {
//
//		return
//	}
//	go UDP_TimerWaitting()
//	PT("[ UDP ] waitting for clients @ UDP:: ")
//	for {
//
//		//read udp
//		//		buf := make([]byte,4096)
//		//		n, remoteAddr, err := conn.ReadFromUDP(buf[0:])
//		body := &util.CombineBody{0, make([]byte, util.BUF_MAX), nil, make([]byte, 0), 0}
//		PT("[ UDP ] recv:1, 等待client...")
//		n, addr, err := connUDP.ReadFromUDP(body.Buffer) //此方法是阻塞的，一直等待客户端的消息
//		PT("[ UDP ] recv:################## 收到新一轮  ##################")
//
//		PT("[ UDP ] recv:2 udp address: " , connUDP.RemoteAddr(),"地址: ", addr)                                  //收到客户端包数据，才走到这里，然后往下走
//		if err != nil {
//			fmt.Fprintf(os.Stderr, "[ UDP ] ######读取失败#####，Fatal error: %s", err.Error())
//			return
//			 //continue
//		}
//
//		fmt.Println("[ UDP ]  udp msg is : ", body.Buffer[0:n], " 接到长度: ", n, "地址: ", addr)
//		go UDP_handler(err, connUDP, body.Buffer, n ,addr)
//
//
//		//udpmsg := make([]byte, 1024)
//		//_, err = conn.WriteToUDP(udpmsg, remoteAddr)
//		//dataMgr.CheckError(err)
//
//	}
//}
//var lockTimer sync.Mutex
//func UDP_TimerWaitting(){
//	timerTick := time.Tick(time.Millisecond * 66) //每隔66毫秒执行一次把本轮收到的所有包列表下发
//	for t := range timerTick {
//		//fmt.Println("TTTTTTTTTTTTT 每隔66毫秒执行一次 , t :", t)
//		for _, d := range dataMgr.Clients_UDP {
//		go dataMgr.UDP_TimerStatusCast(d)
//		}
//
//		t.Clock()
//	}
//}
//func UDP_handler(err error, connUDP *net.UDPConn, buffer []byte, size int , addr *net.UDPAddr) {
//
//	//xxx打印
//	// for i := 0; i < size; i++ {
//	// 	fmt.Println("[ UDP ] :",i, buffer[i])
//	// }
//
//	//检测状态 ===================================
//	//msg := make(chan byte) tcp
//	//go dataMgr.GravelChannel(buffer[:size], msg) tcp
//	//go dataMgr.HeartBeatingChecking_UDP(*conn, msg) for tcp
//	//>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 读取client数据 >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
//	cmd, pb := dataMgr.ReadClient(buffer)
//	//=========================================================================================================
//	pb = dataMgr.GetCloneArry(pb)
//	// ===============  正式按条件解读 客户端业务逻辑  ====================
//	fmt.Println("[ UDP ]>>>>>>>  处理  >>>>>>", cmd)
//	go dataMgr.UDP_Receive(cmd, pb[0:], *connUDP , addr)
//
//}
