'use client'
import React, { useCallback, useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;
import axios from '@/lib/axios';


export default function page() {
  const [sumAB, setSumAB] = useState(0);
  
  const router = useRouter()
  const logOut = () => {
    document.cookie = `token=; path=/; max-age=0;`;
    router.push('/')
  }

  const getData = useCallback(async () => {
    // console.log('object :>> ');
    try {
      const res = await axios.get(`/GetDataAdmin`);
      if (res.status === 200) {
        // console.log('Data fetched successfully:', res.data);
        // console.log('object :>> ', res.data.length);
        // console.log('sumAB :>> ', sumAB);
        // setSumAB(2);
        testSum();
        // console.log('sumAB :>> ', sumAB);
      } else {
        console.error('Failed to fetch data:', res.status, res.statusText);
      }
    } catch (error) {
      console.error('Error fetching data:', error);
    }
  }, []);
  function sum(a: number, b: number) {
    return a + b;
  }

  const testSum = useCallback(async () => {
    // console.log(`sumAB `);

  }, [sumAB]);
  // const get = async () => {
  //   var res = await axios.get(`/GetDataAdmin`);
  // }
  useEffect(() => {
    // get()
    getData()
  }, [])
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-100 via-white to-blue-200 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
      <div className='w-full max-w-sm bg-white dark:bg-gray-800 rounded-xl shadow-lg p-8 space-y-6'>
        <button
          type="button"
          onClick={getData}
          className="w-full text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800 transition"
        >
          test
        </button>
        <button
          type="button"
          onClick={logOut}
          className="w-full text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800 transition"
        >
          Logout
        </button>
      </div>
    </div>
  )
}
