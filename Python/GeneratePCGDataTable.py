def get_neighbors(index):
    neighbors_map = [
        [1, 3, 4],
        [0, 2, 5],
        [1, 3, 6],
        [0, 2, 7],
        [0, 5, 6],
        [1, 4, 7],
        [2, 4, 7],
        [3, 5, 6]
    ]
    return neighbors_map[index]


def get_current_plane(index):
    plane_map = [
        [[3, 7, 4], [1, 5, 4], [1, 2, 3]],
        [[0, 4, 5], [5, 6, 2], [0, 3, 2]],
        [[1, 5, 6], [6, 7, 3], [3, 0, 1]],
        [[7, 4, 0], [7, 6, 2], [0, 1, 2]],
        [[7, 3, 0], [5, 1, 0], [5, 6, 7]],
        [[4, 0, 1], [6, 2, 1], [4, 7, 6]],
        [[5, 1, 2], [7, 3, 2], [7, 4, 5]],
        [[4, 0, 3], [6, 2, 3], [4, 5, 6]]
    ]
    return plane_map[index]


table = []


def generate_table():
    for fill in range(256):
        binary_fill = [int(bit) for bit in format(fill, '08b')]

        for i in range(int(len(binary_fill)/2)):
            tmp = binary_fill[i]
            binary_fill[i] = binary_fill[len(binary_fill)-i-1]
            binary_fill[len(binary_fill) - i - 1] = tmp

        result = set()
        start_filled_state = [-1] * 8
        unmodified_block = set()
        block_count = 0

        for i in range(len(binary_fill)):
            if binary_fill[i] == 1:
                start_filled_state[i] = i
                unmodified_block.add(i)
                block_count = block_count + 1

        # Start fill state: filled x, not filled -1
        def back_track(fill_state):
            is_all_filled = True
            not_filled = set()
            for i, value in enumerate(fill_state):
                if value == -1:
                    is_all_filled = False
                    not_filled.add(i)
            if is_all_filled:
                result.add(tuple(fill_state))
                return
            for block in unmodified_block:
                # Check the whole 3 planes
                for plane in get_current_plane(block):
                    if plane[0] in not_filled and plane[1] in not_filled and plane[2] in not_filled:
                        fill_state[plane[0]] = block
                        fill_state[plane[1]] = block
                        fill_state[plane[2]] = block
                        unmodified_block.remove(block)
                    else:
                        continue

                    back_track(fill_state.copy())

                    # Back track
                    fill_state[plane[0]] = -1
                    fill_state[plane[1]] = -1
                    fill_state[plane[2]] = -1
                    unmodified_block.add(block)

                # Check neighbors
                for neighbor in get_neighbors(block):
                    if neighbor in not_filled:
                        fill_state[neighbor] = block
                        unmodified_block.remove(block)
                    else:
                        continue

                    back_track(fill_state.copy())

                    # Back track
                    fill_state[neighbor] = -1
                    unmodified_block.add(block)

        back_track(start_filled_state.copy())
        table.append(result)


generate_table()


def convert_table_to_list(t):
    final_table = []
    for i in t:
        result = []
        for j in i:
            result.append(list(j))
        final_table.append(result)
    return final_table


def print_table_to_csharp(t):
    result_str = "{"
    for i in t:
        result_str += "new int[][]{"
        for j in i:
            result_str += "new int[]{"
            for k in j:
                result_str += str(k) + ","
            if len(j) != 0:
                result_str = result_str[:-1]
            result_str += "},"
        if len(i) != 0:
            result_str = result_str[:-1]
        result_str += "},\n"
    result_str += "}"
    return result_str


print(convert_table_to_list(table))