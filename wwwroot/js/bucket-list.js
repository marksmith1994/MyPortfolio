window.getBucketListState = function () {
    return localStorage.getItem('bucketListState');
};

window.setBucketListState = function (json) {
    localStorage.setItem('bucketListState', json);
};
