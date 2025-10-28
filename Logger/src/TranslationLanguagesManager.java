import java.io.*;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.Scanner;

public class TranslationLanguagesManager {
    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in);
        while (true)
        {
            Socket s = null;

            System.out.println("What do you want to do? (1) Change your language or (2) Add or remove a target language");
            int in = -1;
            int in2 = -1;
            try{
                in = Integer.parseInt(scanner.nextLine());
            }catch (Exception e)
            {
                System.out.println("Invalid input, enter again!");
                continue;
            }
            if(in != 1 && in != 2) {
                System.out.println("Invalid input, enter again!");
                continue;
            }

            String language;
            if(in == 1)
            {
                System.out.println("Enter the two digit language code:");
                language = scanner.nextLine();
            }else{
                System.out.println("What do you want to do? (1) Remove or (2) Add a target language");

                try{
                    in2 = Integer.parseInt(scanner.nextLine());
                }catch (Exception e)
                {
                    System.out.println("Invalid input, enter again!");
                    continue;
                }
                if(in2 != 1 && in2 != 2) {
                    System.out.println("Invalid input, enter again!");
                    continue;
                }

                System.out.println("Enter the two digit language code:");
                language = scanner.nextLine();
            }

            try{
                s = new Socket("127.0.0.1", 5001);
                OutputStream oS = s.getOutputStream();
                BufferedReader reader = new BufferedReader(new InputStreamReader(s.getInputStream()));

                if(in == 1){
                    oS.write(0);
                    oS.write(language.getBytes(StandardCharsets.UTF_8));
                }else{
                    oS.write(1);
                    oS.write(in2-1);
                    oS.write(language.getBytes(StandardCharsets.UTF_8));
                }

                System.out.print(reader.readLine());
            }catch (IOException e)
            {
                System.err.println("An error occurred while trying to communicate with the LanguageManager, Please try again!");
            }finally {
                try{
                    if(s != null){
                        s.close();
                    }
                }catch (IOException e){
                    System.err.println("An error occurred while trying to close the communication! Message: " + e.getMessage());
                }
            }
            System.out.println("\n");

        }

    }
}
