echo "Building VMA"

BUILD_DIR=build/
BUILD_TYPE=Release

cmake -B "${BUILD_DIR}" -DCMAKE_BUILD_TYPE=${BUILD_TYPE} || exit 1
cmake --build "${BUILD_DIR}" --config ${BUILD_TYPE} || exit 1

cp ${BUILD_DIR}/*VulkanMemoryAllocator.* ../../src/Sprout.Graphics/ || exit 1

echo "Done"
