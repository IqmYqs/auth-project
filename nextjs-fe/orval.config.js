module.exports = {
  myApi: {
    output: {
      mode: 'single',
      target: './src/api/types/service-api.ts',
      client: 'axios',
      mock: false,
      override: {
        // คุณสามารถใส่คอนฟิกเพิ่มเติมได้ตรงนี้หากต้องการ custom client
      }
    },
    input: {
      target: './src/api/service-api.json',
    },
  },
};