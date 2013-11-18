import com.devshorts.enumerable.Enumerable;
import com.devshorts.enumerable.data.Tuple;
import org.junit.Test;

import java.util.List;

import static java.util.Arrays.asList;
import static junit.framework.Assert.assertEquals;

public class EnumerableTests {
    @Test
    public void Distinct(){
        List<Integer> ids = Enumerable.init(asList(1, 1, 1, 2, 3, 4, 5, 6, 6, 7))
                                      .distinct()
                                      .toList();

        assertEquals(ids, asList(1,2,3,4,5,6,7));
    }

    @Test
    public void Zip(){
        Enumerable<Integer> ids = Enumerable.init(asList(1, 2, 3, 4, 5));

        List<String> expectZipped = asList("11", "22", "33", "44", "55");

        assertEquals(expectZipped, ids.zip(ids, (f, s) -> f.toString() + s.toString()).toList());
    }
}
