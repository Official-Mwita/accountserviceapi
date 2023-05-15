// function partition(array, low, high) {
//     let pivot = array[high];
//     let i = low - 1;
//     for (let j = low; j <= high; j++) {
//         if (array[j] < pivot) {
//             i++;
//             swap(array, i, j);
//         }
//     }
//     swap(array, ++i, high)
// }

// function swap(array, i, j) {
//     let temp = array[i];
//     array[i] = array[j]
//     array[j] = temp;
// }


// function quicksort() {
//     let array = [3,6,4,2,7,8,5];
//     let low = 0;
//     let high = array.length - 1;
//     partition(array, low, high);
//     console.log(array);

// }

// quicksort()

function quickSort(array) {
    if (array.length <= 1) {
      return array;
    }
  
    const pivot = array[0];
    const left = [];
    const right = [];
  
    for (let i = 1; i < array.length; i++) {
      if (array[i] < pivot) {
        left.push(array[i]);
      } else {
        right.push(array[i]);
      }
    }
  
    return [...quickSort(left), pivot, ...quickSort(right)];
  }

  
  const unsortedArray = [5, 3, 1, 4, 6, 2];
  const sortedArray = quickSort(unsortedArray);
  console.log(sortedArray); // [1, 2, 3, 4, 5, 6]