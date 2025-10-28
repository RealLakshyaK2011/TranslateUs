import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.ServerSocket;
import java.net.Socket;

public class Logger {
    public static void main(String[] args) {
        ServerSocket logger = null;

        try{
            logger = new ServerSocket(20110);
        }catch (IOException e){
            throw new RuntimeException(e);
        }

        BufferedReader reader;

        while (true){
            try{
                System.out.println("Listening for new connection!");
                Socket s = logger.accept();
                System.out.println("New connection inbound!");

                reader = new BufferedReader(new InputStreamReader(s.getInputStream()));

                while (true){
                    System.out.println("[LOG] -> " + reader.readLine());
                }
            }catch (IOException e){
                System.out.println("Connection outbound!");
            }
        }
    }
}
