import com.devshorts.enumerable.Enumerable;
import com.devshorts.enumerable.data.Tuple;
import com.devshorts.enumerable.data.Yieldable;

import java.util.Iterator;
import java.util.List;
import java.util.concurrent.ExecutionException;
import java.util.function.Supplier;

import static java.util.Arrays.asList;

class Box<T>{
    public T elem;

    public Box(T elem){
        this.elem = elem;
    }
}

public class Main{

    public static void main(String[] arsg) throws InterruptedException, ExecutionException {

        List<String> t = asList("oooo", "ba", "baz", "booo");

        Enumerable<String> strings = Enumerable.init(t);

        Box<Integer> b = new Box(0);

        Enumerable<String> sGen = Enumerable.generate(() -> {

            if(b.elem < 10){
                b.elem++;
                System.out.println("yielding");
                return Yieldable.yield(b.elem.toString());
            }
            else{
                System.out.println("breaking");
                return Yieldable.yieldBreak();
            }
        });

        System.out.println(sGen.fold((acc, elem) -> acc + elem + "foo", ""));
    }
}