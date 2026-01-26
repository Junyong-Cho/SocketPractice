# 프로젝트 구조

## Program.cs

- 메인 파일
- 미구현

## RecvBuffer

- 패킷 버퍼
- raw buffer, buffer, 현재까지 읽은 위치, 현재까지 작성한 위치
- 데이터 크기, 여유 공간 크기
- 읽기용 쓰기용 세그먼트 반환
- 읽고 쓴 후 각 포인터 옮기기
- 버퍼가 꽉 차면 포인터 앞으로 옮기기
- IDisposable 구현

## Session.cs

- 패킷 처리 쓰레드?
- Socket, RecvBuffer, Queue, List
- object, bool
- 소켓을 통해 비트 받고 패킷 덩어리 한번에 처리하면서 루프
- 패킷을 전송하는 로직 구현
- 스레드 동기화 구현

## PacketHeader.cs

- 패킷 상태 정의 구조체
- 패킷 사이즈, Id
- 구조체 자체 사이즈 반환

## SendBuffer.cs

- 내부에 byte[] 버퍼를 가지고 Open과 Close로 ArraySegment 전달하며 포인터 옮김
- FreeSize보다 큰 용량 요구시 null 리턴

## SendBufferHandler

- ThreadLocal&lt;SendBuffer&gt;를 가지며 쓰레드마다 독립적인 SendBuffer 작업공간 가짐
- Open과 Close로 SendBuffer의 ArraySegment 전달 역할
- null을 return받으면 SendBuffer 인스턴스 새로 생성