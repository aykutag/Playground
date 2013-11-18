import com.devshorts.enumerable.Enumerable;
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
}
