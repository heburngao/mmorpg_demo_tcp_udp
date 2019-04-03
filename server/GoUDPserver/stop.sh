#!/bin/sh
	PID=`ps -ef | grep tcpGate | grep -v grep | awk '{print $2}'`
    	if [ "" != "$PID" ]; then
    	 echo "killing $PID"
    	 kill -9 $PID
    	else
    	 echo "tcpGate not running!"
    	fi
