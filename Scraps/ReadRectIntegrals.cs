#if false
public static double[][][][] ReadRectStiffnessIntegrals(string relativePath) {
         var file = new FileInfo(relativePath);
         var results = new double[12][][][];

         for(int j = 0; j < 12; ++j) {
            results[j] = new double[12-j][][];

            for(int k = 0; k < 12-j; ++k) {
               results[j][k] = new double[5][];

               for(int n = 0; n < 5; ++n) {
                  results[j][k][n] = new double[5] {0,0,0,0,0};
               }
            }
         }
         var dimIndex = new int[] {0,0,0,0};     // Indices of levels at which we reside.
         int currLevel = -1;
         char prevRead = 'č', currRead;
         double number;
         var numberSB = new StringBuilder(30);
         var numberFormatInfo = new NumberFormatInfo();
         numberFormatInfo.NumberDecimalSeparator = ".";

         using (var sr = new StreamReader(file.FullName)) {

            while(sr.Peek() > -1) {
               currRead = (char)sr.Read();
               
               if(currRead == '{') {                               // If it's an opening brace.
                  ++currLevel;
               }
               else if(currRead == ',') {                          // Delimits dimensions.

                  if(prevRead == '.') {                      // Dumb-ass Mathematica export format where 0., is a posibility.
                     number = 0.0;
                     results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]] = number;
                     numberSB.Clear();
                  }
                  else if(IsDigit(prevRead)) {                         // CurrRead is a comma and prevRead was a digit. We stay at same level, but write prevRead.
                     number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                     results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]] = number;     // Write number.
                     numberSB.Clear();                           // Clear string for next number.
                  }
                  ++dimIndex[currLevel];                           // Advance an index in any case (number or closing bracket).
               }
               else if(currRead == '}') {

                  if(IsDigit(prevRead) || prevRead == '.') {                         // We encountered a closing brace after a digit or decimals delimiter. Flush string.
                     number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                     results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]] = number;
                     numberSB.Clear();
                  }
                  dimIndex[currLevel] = 0;                         // Leaving a level, reset its index.
                  --currLevel;
               }
               else if(IsDigit(currRead) || currRead == '.' || currRead == '-') {                    // If it's a digit or decimals separator,
                  numberSB.Append(currRead);                      // Append currRead in all cases.
               }
               else if(currRead == '^') {
                  numberSB.Append('E');
               }
               prevRead = currRead;                                // Prepare for next iteration.
         }  }
         return results;
      }

        /// <summary>Reads Mathematica exported integrals needed to compute forcing vector which are produced by a single element of rectangular grid.</summary><param name="relativePath">Path to exported integrals.</param><remarks>[j][n] = [12 basis functions][5 term parts]</remarks>
        public static double[][] ReadRectForcingIntegrals(string relativePath) {
            var file = new FileInfo(relativePath);
            var results = new double[12][];

            for(int j = 0; j < 12; ++j) {
                results[j] = new double[5] {0,0,0,0,0};
            }
            var dimIndex = new int[] {0,0};     // Indices of levels at which we reside.
            int currLevel = -1;
            char prevRead = 'č', currRead;
            double number;
            var numberSB = new StringBuilder(30);
            var numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";

            using (var sr = new StreamReader(file.FullName)) {

                while(sr.Peek() > -1) {
                    currRead = (char)sr.Read();
                    
                    if(currRead == '{') {                               // If it's an opening brace.
                        ++currLevel;
                    }
                    else if(currRead == ',') {                          // Delimits dimensions.

                        if(prevRead == '.') {                      // Dumb-ass Mathematica export format where 0., is a posibility.
                            number = 0.0;
                            results[dimIndex[0]][dimIndex[1]] = number;
                            numberSB.Clear();
                        }
                        else if(IsDigit(prevRead)) {                         // CurrRead is a comma and prevRead was a digit. We stay at same level, but write prevRead.
                            number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                            results[dimIndex[0]][dimIndex[1]] = number;     // Write number.
                            numberSB.Clear();                           // Clear string for next number.
                        }
                        ++dimIndex[currLevel];                           // Advance an index in any case (number or closing bracket).
                    }
                    else if(currRead == '}') {

                        if(IsDigit(prevRead) || prevRead == '.') {                         // We encountered a closing brace after a digit or decimals delimiter. Flush string.
                            number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                            results[dimIndex[0]][dimIndex[1]] = number;
                            numberSB.Clear();
                        }
                        dimIndex[currLevel] = 0;                         // Leaving a level, reset its index.
                        --currLevel;
                    }
                    else if(IsDigit(currRead) || currRead == '.' || currRead == '-') {                    // If it's a digit or decimals separator,
                        numberSB.Append(currRead);                      // Append currRead in all cases.
                    }
                    else if(currRead == '^') {
                        numberSB.Append('E');
                    }
                    prevRead = currRead;                                // Prepare for next iteration.
                }
            }
            return results;
        }
#endif