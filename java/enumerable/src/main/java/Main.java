import com.devshorts.enumerable.Enumerable;
import java.util.List;
import java.util.concurrent.ExecutionException;

import static java.util.Arrays.asList;

public class Main{
    public static void main(String[] arsg) throws InterruptedException, ExecutionException {
        List<String> strings = asList("o", "ba", "baz", "booo");

        List<String> items = Enumerable.init(strings)
                .skip(1)
                .take(20)
                .takeWhile(i -> i.length() <= 3)
                .map(i -> "three!")
                .toList();

        for(String x : items){
            System.out.println(x);
        }
    }
}