import com.devshorts.enumerable.Enumerable;
import java.util.List;
import java.util.concurrent.ExecutionException;

import static java.util.Arrays.asList;

public class Main{
    public static void main(String[] arsg) throws InterruptedException, ExecutionException {
        List<String> strings = asList("oooo", "ba", "baz", "booo");

        Enumerable.init(strings)
            .orderBy(String::length)
            .map(String::length)
            .iter(System.out::println)
            .iteri((indx, length) -> System.out.println(("index" + indx + " length " + length)))
            .toList();
    }
}