import com.devshorts.enumerable.Enumerable;
import java.util.List;
import java.util.concurrent.ExecutionException;

import static java.util.Arrays.asList;

public class Main{
    public static void main(String[] arsg) throws InterruptedException, ExecutionException {
        List<String> strings = asList("oooo", "ba", "baz", "booo");

        Enumerable<String, String> items = Enumerable.init(strings)
                                                     .orderBy(i -> i.length());

        for(String x : items){
            System.out.println(x);
        }

        for(String x : items){
            System.out.println(x);
        }
    }
}