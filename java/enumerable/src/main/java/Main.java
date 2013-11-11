import com.devshorts.enumerable.Enumerable;
import java.util.List;
import java.util.concurrent.ExecutionException;

import static java.util.Arrays.asList;

public class Main{
    public static void main(String[] arsg) throws InterruptedException, ExecutionException {
        List<String> strings = asList("oooo", "ba", "baz", "booo");

        Enumerable<Integer> items = Enumerable.init(strings)
                                                .orderBy(i -> i.length())
                                                .map(i -> i.length())
                                                .filter(i -> i == 2);

        for(Integer x : items){
            System.out.println(x);
        }

        for(Integer x : items){
            System.out.println(x);
        }
    }
}