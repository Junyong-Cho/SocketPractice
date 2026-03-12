# ServerCore
네트워크 엔진 라이브러리

# 아키텍처 구조

## Buffers

### RecvBuffer.cs
<img width="1628" height="421" alt="image" src="https://github.com/user-attachments/assets/f6c15a3b-b58f-42ec-98e3-4bd0fe2302e4" />

- 패킷 수신 버퍼
- ArraySegment<byte> ReadSegment
  - 수신된 데이터 중 처리되지 않은 데이터
- ArraySegment<byte> WriteSegment
  -  소켓을 통해 전송 받을 버퍼
- int DataSize
  - ReadSegment 사이즈
- int FreeSize
  - WriteSegment 사이즈
- bool OnRecv(int readSize)
  - ReadSegment에서 처리한 바이트만큼 ReadPos 포인터 이동 (DataSize보다 크면 false)
- bool OnSend(int writeSize)
  - WriteSegment에서 처리한 바이트만큼 WritePos 포인터 이동 (FreeSize보다 크면 false)
- void Clean()
  - 포인터가 어느 정도 차면 ReadSegment를 배열의 맨 앞으로 복사 후 포인터 조정
- void Reset()
  - 버퍼 리셋, 각 포인터 0으로 초기화
  
### SendBuffer.cs
<img width="1631" height="384" alt="image" src="https://github.com/user-attachments/assets/e27a1d6c-eaa1-429b-8b28-8f1071faaff0" />

- 패킷 전송 버퍼
- int FreeSize : 버퍼의 남은 사이즈
- ArraySegment<byte>? Open(int reserveSize) :
  - 전송할 패킷을 직렬하기 위해 버퍼에서 특정 크기만큼 예약
  - FreeSize보다 예약한 크기가 클 경우 null 반환
- ArraySegment<byte>? Close(int usedCount) :
  - 직렬화한 크기만큼 버퍼 슬라이싱 및 반환, UsedSize 포인터 이동
  - FreeSize보다 사용한 크기가 클 경우 null 반환

**추후 ArrayPool과 reference count로 힙 할당 최소화 예정**

### SendBufferHandler.cs
<img width="1602" height="392" alt="image" src="https://github.com/user-attachments/assets/5aacdc1c-eca4-46ac-859a-79140e96e781" />

- SendBuffer를 다루는 정적 클래스
- ThreadLocal<SendBuffer>
  - 세션에서 버퍼를 전송할 때 랜덤한 쓰레드에서 전송하게 됨
  - 각 쓰레드별로 별도의 SendBuffer를 가지도록 하여 버퍼 안정화
- ArraySegment<byte> Open(int reserveSize)
  - SendBuffer의 Open 호출 및 반환, null 반환 시 새로운 SendBuffer 생성
  - SendBuffer의 버퍼사이즈보다 큰 크기로 예약시 예외 발생
- ArraySegment<byte> Close(int usedSize)
  - SendBuffer의 Close 호출 및 반환
  - null 반환 시 예외 발생 (Open 후 Close 강제)

## Sessions

### Session.cs
- 추상 클래스
- abstract void OnConnect()
  - 세션 활성화시 호출되어 세션 Reset 등 초기화 구현
- abstract void OnDisconnect()
  - 세션 종료시 호출되어 세션 풀 반납 등 구현
- abstract int OnRecv(ArraySegment<byte> segment)
  - Recieve 이벤트 발생시 전송받은 패킷을 처리하고 처리한 사이즈 반환 (이후 RecvBuffer의 OnRead 호출)
- abstract int OnSend(int numOfBytes)
  - 추후 SendBuffer의 refCount 구현을 위한 메서드 ~(미구현)~
- RegisterSend()
  - 소켓에서 데이터를 수신할 때 이벤트를 발생하도록 등록
  - 
- 가독성과 유지보수를 위해 SendSession.cs, RecvSession.cs, SessionMain.cs로 나누어짐
- SocketAsyncEventArgs로 비동기 힙 할당을 최소화하는 고성능 모델
- _sendingList와 _pendingList 두 개의 리스트 참조를 스왑하며 lock 시간 단축

### SessionPool.cs
- 연결 종료된 세션을 재사용하기 위한 풀링 클래스

# 테스트 프로젝트

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
