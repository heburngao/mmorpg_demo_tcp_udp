# /////////////////////////////////////////////////////////////////////////////
#
# 编译Protobuf2协议

#安装proto for go 支持库
#go get github.com/golang/protobuf/proto
#cd github.com/golang/protobuf/proto
#go build
#go install 
# protobuff v2.6.1
#
# /////////////////////////////////////////////////////////////////////////////

protoc --version

protoc --go_out=../msg  *.proto

echo "*.proto -> *.go finished ~!"