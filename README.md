# 프로젝트 구조

클라이언트가 서버로 메세지를 보내면 서버에서 그 메세지를 그대로 반송하는 테스트 서버

## ServerCore
네트워크 엔진 라이브러리

### Buffers

#### RecvBuffer.cs
- 수신 패킷 버퍼
- 링 버퍼 형태

#### SendBuffer.cs
- 전송 패킷 버퍼
- 버퍼가 가득 차면 새로운 버퍼 할당
- 추후 ArrayPool과 reference count로 할당 최소화 예정

#### SendBufferHandler.cs
- SendBuffer를 다루는 정적 클래스
- 여러 쓰레드에서 패킷을 전송할 것이므로 각 쓰레드마다 독립적인 SendBuffer를 가지도록 ThreadLocal<SendBuffer?>로 관리

### Sessions

#### Session.cs
- 추상 클래스
- 가독성과 유지보수를 위해 SendSession.cs, RecvSession.cs, SessionMain.cs로 나누어진 분할 클래스
- SocketAsyncEventArgs로 비동기 힙 할당을 최소화하는 고성능 모델
- _sendingList와 _pendingList 두 개의 리스트 참조를 스왑하며 lock 시간 단축

#### SessionPool.cs
- 연결 종료된 세션을 재사용하기 위한 풀링 클래스

## Client

### Program.cs
- 클라이언트 프로그램 최상위 문

### ClientSession
- 클라이언트 사이드 Session 클래스 구현

## Server

### Program.cs
- 서버 프로그램 최상위 문

### ClientListener.cs
- SocketAsyncEventArgs를 이용한 소켓 리스너
- 연결 성공시 SessionPool에서 세션 대여

### ServerSession.cs
- 서버 사이드 Session 클래스 구현
- OnDisconnect에서 SessionPool에 세션 반납