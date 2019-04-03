package constant

import (
	"os"
	"fmt"
)

const (
	//_ Enum = iota
	//Enum            	= iota
	TCP_Enum_HeartBeat            = 110
	TCP_Enum_CreateSelf           = 10000
	TCP_Enum_CreateOtherPlayer    = 10001
	UDP_Enum_UpdateStatus         = 20002
	UDP_Enum_UpdateStatus_Confirm = 20003
	TCP_Enum_OthersLineOff        = 10003
	TCP_Enum_AddRoboot            = 99999
)
func CheckError(err error) {
	if err != nil {
		fmt.Fprintf(os.Stderr, "Fatal error: %s", err.Error())
		os.Exit(1)
	}
}