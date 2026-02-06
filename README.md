# 프로젝트 구조

## Program.cs

- 메인 파일

## ClientListener.cs

- 클라이언트 소켓 처리 클래스
- SAEA로 구현
- Start와 OnAcceptComplete 구현

## RecvBuffer

- 패킷 버퍼
- raw buffer, buffer, 현재까지 읽은 위치, 현재까지 작성한 위치
- 데이터 크기, 여유 공간 크기
- 읽기용 쓰기용 세그먼트 반환
- 읽고 쓴 후 각 포인터 옮기기
- 버퍼가 일정 부분 차면 포인터 앞으로 옮기기

## Session.cs

- 메인 소켓 처리 추상 클래스
- Send 코드(SendSession), Recv 코드(RecvSession), 필드 추상 메서드 등 구현 파일(SessionMain)로 구분해 관리
- SessionMain
    - 잠금 오브젝트
    - 연결 상태 여부 변수
    - 소켓
    - RecvBuffer
    - 임시 큐, 전송 리스트
    - send, recv SAEA
    - 연결, 연결 종료, Send 성공 시, Recv 성공 시 동작할 추상 메서드 정의
    - 세션 시작 메서드
- SendSession
    - Send 메서드(큐에 저장)
    - Send 등록 메서드(전송 시작)
    - Send 성공시 동작할 메서드
- RecvSession
    - Recv 등록 메서드 - 시작시 바로 동작
    - Recv 성공시 동작할 메서드


## SendBuffer.cs

- 내부에 byte[] 버퍼를 가지고 Open과 Close로 ArraySegment 전달하며 포인터 옮김
- FreeSize보다 큰 용량 요구시 null 리턴

## SendBufferHandler

- ThreadLocal&lt;SendBuffer&gt;로 쓰레드마다 독립적인 SendBuffer 작업공간 가짐
- Open과 Close로 SendBuffer의 ArraySegment 전달 역할
- null을 return받으면 SendBuffer 인스턴스 새로 생성

# Gemini 피드백 내용

- RecvBuffer에서 내어주는 WriteSegment 프로퍼티에서 세그먼트를 내어주기 전 남은 버퍼 크기를 계산하여 세그먼트를 청소하는 로직을 구현하였는데 프로퍼티는 가벼운 작업을 하는 것이 C# 개발자들의 약속이며 low level의 서버에서는 개발자가 명시적으로 청소를 하도록 하는 것이 이상적이라고 했다.

- Socket은 늘 불안정하기에 Session에서 소켓을 사용하는 코드는 전부 try catch문으로 감싸주고 catch문에 도달했다는 것은 소켓에 이상이 생겼다는 점으로 반드시 Disconnect를 호출하여 세션을 종료하도록 한다.
- Disconnect에 socket.Close()를 깜빡했다.

- _disconnect 변수를 검사하는 과정을 lock으로 감싸주어 검사중에 _disconnect가 변경되는 것을 대비했는데 이는 검사 효율 대비 비용이 큰 코드로 _disconnect 변수를 volatile로 선언하여 변수를 캐싱하지 않고 매번 메모리를 직접 참조하여 변경을 감지하도록 하였다.