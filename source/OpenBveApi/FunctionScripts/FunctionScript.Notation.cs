using System;

namespace OpenBveApi.FunctionScripting
{
	/// <summary>Contains functions for dealing with function script notation</summary>
	public class FunctionScriptNotation
	{
		/// <summary>Converts a string formatted in simple function script to postfix notation</summary>
		/// <param name="Expression">The function script string</param>
		public static string GetPostfixNotationFromFunctionNotation(string Expression) {
			int i = Expression.IndexOf('[');
			if (i >= 0) {
				if (!Expression.EndsWith("]")) {
					throw new System.IO.InvalidDataException("Missing closing bracket encountered in " + Expression);
				}
			} else {
				if (Expression.EndsWith("]")) {
					throw new System.IO.InvalidDataException("Unexpected closing bracket encountered in " + Expression);
				}
				// ReSharper disable once NotAccessedVariable
				/*
				 * If this is a simple number, we can short-circuit the rest of this function
				 */
				double value; 
				if (double.TryParse(Expression, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value)) {
					return Expression;
				}
				for (int j = 0; j < Expression.Length; j++) {
					if (!char.IsLetterOrDigit(Expression[j]) && Expression[j] != ':') {
						throw new System.IO.InvalidDataException("Invalid character encountered in variable " + Expression);
					}
				}
				return Expression;
			}
			string f = Expression.Substring(0, i);
			string s = Expression.Substring(i + 1, Expression.Length - i - 2);
			string[] a = new string[4];
			int n = 0;
			int b = 0;
			for (i = 0; i < s.Length; i++) {
				switch (s[i]) {
					case '[':
						{
							i++; int m = 1;
							bool q = false;
							while (i < s.Length) {
								switch (s[i]) {
									case '[':
										m++;
										break;
									case ']':
										m--;
										if (m < 0) {
											throw new System.IO.InvalidDataException("Unexpected closing bracket encountered in " + Expression);
										} 
										if (m == 0) {
											q = true;
										}
										break;
								}
								if (q) {
									break;
								}
								i++;
							} if (!q) {
								throw new System.IO.InvalidDataException("No closing bracket found in " + Expression);
							}
						} break;
					case ']':
						throw new System.IO.InvalidDataException("Unexpected closing bracket encountered in " + Expression);
					case ',':
						if (n == a.Length) {
							Array.Resize<string>(ref a, n << 1);
						}
						a[n] = s.Substring(b, i - b).Trim();
						n++;
						b = i + 1;
						break;
				}
			}
			if (n == a.Length) {
				Array.Resize<string>(ref a, n << 1);
			}
			a[n] = s.Substring(b).Trim();
			n++;
			if (n == 1 & a[0].Length == 0) {
				n = 0;
			}
			for (i = 0; i < n; i++) {
				if (a[i].Length == 0) {
					throw new System.IO.InvalidDataException("An empty argument is invalid in " + f + " in " + Expression);
				} 
				if (a[i].IndexOf(' ') >= 0) {
					throw new System.IO.InvalidDataException("An argument containing a space is invalid in " + f + " in " + Expression);
				}
				a[i] = GetPostfixNotationFromFunctionNotation(a[i]).Trim();
			}
			switch (f.ToLowerInvariant()) {
					// arithmetic
				case "plus":
					switch (n)
					{
						case 0:
							return "0";
						case 1:
							return a[0];
						case 2:
							if (a[1].EndsWith(" *")) {
								return a[1] + " " + a[0] + " +";
							} else {
								return a[0] + " " + a[1] + " +";
							}
						default:
							System.Text.StringBuilder t = new System.Text.StringBuilder(a[0] + " " + a[1] + " +");
							for (i = 2; i < n; i++) {
								t.Append(" " + a[i] + " +");
							}
							return t.ToString();
					}
				case "subtract":
					if (n == 2) {
						return a[0] + " " + a[1] + " -";
					} 
					throw new System.IO.InvalidDataException(f + " is expected to have 2 arguments in " + Expression);
				case "times":
					switch (n)
					{
						case 0:
							return "1";
						case 1:
							return a[0];
						case 2:
							return a[0] + " " + a[1] + " *";
						default:
							System.Text.StringBuilder t = new System.Text.StringBuilder(a[0] + " " + a[1] + " *");
							for (i = 2; i < n; i++) {
								t.Append(" " + a[i] + " *");
							}
							return t.ToString();
					}
				case "divide":
					if (n == 2) {
						return a[0] + " " + a[1] + " /";
					} 
					throw new System.IO.InvalidDataException(f + " is expected to have 2 arguments in " + Expression);
				case "power":
					switch (n)
					{
						case 0:
							return "1";
						case 1:
							return a[0];
						case 2:
							return a[0] + " " + a[1] + " power";
						default:
							System.Text.StringBuilder t = new System.Text.StringBuilder(a[0] + " " + a[1]);
							for (i = 2; i < n; i++) {
								t.Append(" " + a[i]);
							}
							for (i = 0; i < n - 1; i++) {
								t.Append(" power");
							}
							return t.ToString();
					}
					// math
				case "random":
				case "randomint":
					if (n == 2)
					{
						return a[0] + " " + a[1] + " " + f;
					}
					throw new System.IO.InvalidDataException(f + " is expected to have 2 arguments in " + Expression);
				case "quotient":
				case "mod":
				case "min":
				case "max":
					if (n == 2) {
						return a[0] + " " + a[1] + " " + f;
					} 
					throw new System.IO.InvalidDataException(f + " is expected to have 2 arguments in " + Expression);
				case "minus":
				case "reciprocal":
				case "floor":
				case "ceiling":
				case "round":
				case "abs":
				case "sign":
				case "exp":
				case "log":
				case "sqrt":
				case "sin":
				case "cos":
				case "tan":
				case "arctan":
					if (n == 1) {
						return a[0] + " " + f;
					} 
					throw new System.IO.InvalidDataException(f + " is expected to have 1 argument in " + Expression);
					// comparisons
				case "equal":
				case "unequal":
				case "less":
				case "greater":
				case "lessequal":
				case "greaterequal":
					if (n == 2) {
						string g; switch (f.ToLowerInvariant()) {
								case "equal": g = "=="; break;
								case "unequal": g = "!="; break;
								case "less": g = "<"; break;
								case "greater": g = ">"; break;
								case "lessequal": g = "<="; break;
								case "greaterequal": g = ">="; break;
								default: g = "halt"; break;
						}
						return a[0] + " " + a[1] + " " + g;
					}
					throw new System.IO.InvalidDataException(f + " is expected to have 2 arguments in " + Expression);
				case "if":
					if (n == 3) {
						return a[0] + " " + a[1] + " " + a[2] + " ?";
					}
					throw new System.IO.InvalidDataException(f + " is expected to have 3 arguments in " + Expression);
					// logical
				case "not":
					if (n == 1) {
						return a[0] + " !";
					}
					throw new System.IO.InvalidDataException(f + " is expected to have 1 argument in " + Expression);
				case "and":
					switch (n)
					{
						case 0:
							return "1";
						case 1:
							return a[0];
						case 2:
							return a[0] + " " + a[1] + " &";
						default:
							System.Text.StringBuilder t = new System.Text.StringBuilder(a[0] + " " + a[1] + " +");
							for (i = 2; i < n; i++) {
								t.Append(" " + a[i] + " &");
							} return t.ToString();
					}
				case "or":
					switch (n)
					{
						case 0:
							return "0";
						case 1:
							return a[0];
						case 2:
							return a[0] + " " + a[1] + " |";
						default:
							System.Text.StringBuilder t = new System.Text.StringBuilder(a[0] + " " + a[1] + " +");
							for (i = 2; i < n; i++) {
								t.Append(" " + a[i] + " |");
							} return t.ToString();
					}
				case "xor":
					switch (n)
					{
						case 0:
							return "0";
						case 1:
							return a[0];
						case 2:
							return a[0] + " " + a[1] + " ^";
						default:
							System.Text.StringBuilder t = new System.Text.StringBuilder(a[0] + " " + a[1] + " +");
							for (i = 2; i < n; i++) {
								t.Append(" " + a[i] + " ^");
							} return t.ToString();
					}
					// train
				case "distance":
				case "trackdistance":
				case "curveradius":
				case "frontaxlecurveradius":
				case "rearaxlecurveradius":
				case "curvecant":
				case "pitch":
				case "odometer":
				case "speed":
				case "speedometer":
				case "acceleration":
				case "accelerationmotor":
				case "doors":
				case "leftdoors":
				case "rightdoorstarget":
				case "leftdoorstarget":
				case "rightdoors":
				case "mainreservoir":
				case "equalizingreservoir":
				case "brakepipe":
				case "brakecylinder":
				case "straightairpipe":
					if (n == 1) {
						return a[0] + " " + f.ToLowerInvariant() + "index";
					}
					throw new System.IO.InvalidDataException(f + " is expected to have 1 argument in " + Expression);
				case "pluginstate":
					if (n == 1) {
						return a[0] + " pluginstate";
					} 
					throw new System.IO.InvalidDataException(f + " is expected to have 1 argument in " + Expression);
					// not supported
				default:
					throw new System.IO.InvalidDataException("The function " + f + " is not supported in " + Expression);
			}
		}

		/// <summary>Converts a string formatted in infix notation to postfix notation</summary>
		/// <param name="Expression">The function script string</param>
		public static string GetPostfixNotationFromInfixNotation(string Expression) {
			string Function = FunctionScriptNotation.GetFunctionNotationFromInfixNotation(Expression, true);
			return GetPostfixNotationFromFunctionNotation(Function);
		}

		/// <summary>Gets the optimized version of a postfix notated function script</summary>
		/// <param name="Expression">The function script string to optimize</param>
		internal static string GetOptimizedPostfixNotation(string Expression) {
			Expression = " " + Expression + " ";
			Expression = Expression.Replace(" 1 1 == -- ", " 0 ");
			Expression = Expression.Replace(" 1 doors - 1 == -- ", " doors ! -- ");
			string[] Arguments = Expression.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string[] Stack = new string[Arguments.Length];
			int StackLength = 0;
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			for (int i = 0; i < Arguments.Length; i++) {
				switch (Arguments[i].ToLowerInvariant()) {
					case "<>":
						{
							bool q = true;
							if (StackLength >= 1) {
								if (Stack[StackLength - 1] == "<>") {
									// <> <>
									// [n/a]
									StackLength--;
									q = false;
								} else if (StackLength >= 2) {
									double b;
									if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
										double a;
										if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// a b <>
											// b a
											string t = Stack[StackLength - 1];
											Stack[StackLength - 1] = Stack[StackLength - 2];
											Stack[StackLength - 2] = t;
										}
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "+":
						{
							bool q = true;
							if (StackLength >= 2) {
								double b;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
									double a;
									if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
										// x y +
										// (x y +)
										Stack[StackLength - 2] = (a + b).ToString(Culture);
										StackLength--;
										q = false;
									} else if (StackLength >= 3 && Stack[StackLength - 2] == "+") {
										if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// A x + y +
											// A (y x +) +
											Stack[StackLength - 3] = (a + b).ToString(Culture);
											StackLength--;
											q = false;
										}
									} else if (StackLength >= 3 && Stack[StackLength - 2] == "-") {
										if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// A x - y +
											// A (y x -) +
											Stack[StackLength - 3] = (b - a).ToString(Culture);
											Stack[StackLength - 2] = "+";
											StackLength--;
											q = false;
										}
									} else if (Stack[StackLength - 2] == "*") {
										// A x * y +
										// A x y fma
										Stack[StackLength - 2] = Stack[StackLength - 1];
										Stack[StackLength - 1] = "fma";
										q = false;
									} else if (Stack[StackLength - 2] == "fma") {
										if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// A B y fma z +
											// A B (y z +) fma
											Stack[StackLength - 3] = (a + b).ToString(Culture);
											StackLength--;
											q = false;
										}
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "-":
						{
							bool q = true;
							if (StackLength >= 2) {
								double b;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
									double a;
									if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
										// x y -
										// (x y -)
										Stack[StackLength - 2] = (a - b).ToString(Culture);
										StackLength--;
										q = false;
									} else if (StackLength >= 3 && Stack[StackLength - 2] == "+") {
										if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// A x + y -
											// A (x y -) +
											Stack[StackLength - 3] = (a - b).ToString(Culture);
											Stack[StackLength - 2] = "+";
											StackLength--;
											q = false;
										}
									} else if (StackLength >= 3 && Stack[StackLength - 2] == "-") {
										if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// A x - y -
											// A (x y + minus) -
											Stack[StackLength - 3] = (-a - b).ToString(Culture);
											Stack[StackLength - 2] = "+";
											StackLength--;
											q = false;
										}
									} else if (Stack[StackLength - 2] == "*") {
										// A x * y -
										// A x (y minus) fma
										Stack[StackLength - 2] = (-b).ToString(Culture);
										Stack[StackLength - 1] = "fma";
										q = false;
									} else if (Stack[StackLength - 2] == "fma") {
										if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// A B y fma z -
											// A B (y z -) fma
											Stack[StackLength - 3] = (a - b).ToString(Culture);
											StackLength--;
											q = false;
										}
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "minus":
						{
							bool q = true;
							if (StackLength >= 1) {
								if (Stack[StackLength - 1].Equals("minus", StringComparison.InvariantCultureIgnoreCase)) {
									// minus minus
									// [n/a]
									StackLength--;
									q = false;
								} else {
									double a;
									if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out a)) {
										// x minus
										// (x minus)
										Stack[StackLength - 1] = (-a).ToString(Culture);
										q = false;
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "*":
						{
							bool q = true;
							if (StackLength >= 2) {
								double b;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
									double a;
									if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
										// x y *
										// (x y *)
										Stack[StackLength - 2] = (a * b).ToString(Culture);
										StackLength--;
										q = false;
									} else if (StackLength >= 3 && Stack[StackLength - 2] == "*") {
										if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// A x * y *
											// A (x y *) *
											Stack[StackLength - 3] = (a * b).ToString(Culture);
											StackLength--;
											q = false;
										}
									} else if (StackLength >= 3 && Stack[StackLength - 2] == "+") {
										if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// A x + y *
											// A y (x y *) fma
											Stack[StackLength - 3] = Stack[StackLength - 1];
											Stack[StackLength - 2] = (a * b).ToString(Culture);
											Stack[StackLength - 1] = "fma";
											q = false;
										}
									} else if (StackLength >= 3 && Stack[StackLength - 2] == "-") {
										if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// A x - y *
											// A y (x y * minus) fma
											Stack[StackLength - 3] = Stack[StackLength - 1];
											Stack[StackLength - 2] = (-a * b).ToString(Culture);
											Stack[StackLength - 1] = "fma";
											q = false;
										}
									} else if (StackLength >= 4 && Stack[StackLength - 2] == "fma") {
										if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
											double c;
											if (double.TryParse(Stack[StackLength - 4], System.Globalization.NumberStyles.Float, Culture, out c)) {
												// A x y fma z *
												// A (x z *) (y z *) fma
												Stack[StackLength - 4] = (c * b).ToString(Culture);
												Stack[StackLength - 3] = (a * b).ToString(Culture);
												StackLength--;
												q = false;
											} else {
												// A B y fma z *
												// A B * z (y z *) fma
												Stack[StackLength - 3] = "*";
												Stack[StackLength - 2] = Stack[StackLength - 1];
												Stack[StackLength - 1] = (a * b).ToString(Culture);
												Stack[StackLength] = "fma";
												StackLength++;
												q = false;
											}
										}
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "reciprocal":
						{
							bool q = true;
							if (StackLength >= 1) {
								if (Stack[StackLength - 1].Equals("reciprocal", StringComparison.InvariantCultureIgnoreCase)) {
									// reciprocal reciprocal
									// [n/a]
									StackLength--;
									q = false;
								} else {
									double a;
									if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out a)) {
										// x reciprocal
										// (x reciprocal)
										a = a == 0.0 ? 0.0 : 1.0 / a;
										Stack[StackLength - 1] = a.ToString(Culture);
										q = false;
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "/":
						{
							bool q = true;
							if (StackLength >= 2) {
								double b;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
									if (b != 0.0) {
										double a;
										if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
											// x y /
											// (x y /)
											Stack[StackLength - 2] = (a / b).ToString(Culture);
											StackLength--;
											q = false;
										} else if (StackLength >= 3 && Stack[StackLength - 2] == "*") {
											if (double.TryParse(Stack[StackLength - 3], System.Globalization.NumberStyles.Float, Culture, out a)) {
												// A x * y /
												// A (x y /) *
												Stack[StackLength - 3] = (a / b).ToString(Culture);
												StackLength--;
												q = false;
											}
										}
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "++":
						{
							bool q = true;
							if (StackLength >= 1) {
								double a;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out a)) {
									// x ++
									// (x ++)
									Stack[StackLength - 1] = (a + 1).ToString(Culture);
									q = false;
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "--":
						{
							bool q = true;
							if (StackLength >= 1) {
								double a;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out a)) {
									// x --
									// (x --)
									Stack[StackLength - 1] = (a - 1).ToString(Culture);
									q = false;
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "!":
						{
							bool q = true;
							if (StackLength >= 1) {
								if (Stack[StackLength - 1] == "!") {
									StackLength--;
									q = false;
								} else if (Stack[StackLength - 1] == "==") {
									Stack[StackLength - 1] = "!=";
									q = false;
								} else if (Stack[StackLength - 1] == "!=") {
									Stack[StackLength - 1] = "==";
									q = false;
								} else if (Stack[StackLength - 1] == "<") {
									Stack[StackLength - 1] = ">=";
									q = false;
								} else if (Stack[StackLength - 1] == ">") {
									Stack[StackLength - 1] = "<=";
									q = false;
								} else if (Stack[StackLength - 1] == "<=") {
									Stack[StackLength - 1] = ">";
									q = false;
								} else if (Stack[StackLength - 1] == ">=") {
									Stack[StackLength - 1] = "<";
									q = false;
								} else {
									double a;
									if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out a)) {
										Stack[StackLength - 1] = a == 0.0 ? "1" : "0";
										q = false;
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "==":
						{
							bool q = true;
							if (StackLength >= 2) {
								double b;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
									double a;
									if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
										Stack[StackLength - 2] = a == b ? "1" : "0";
										StackLength--;
										q = false;
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "!=":
						{
							bool q = true;
							if (StackLength >= 2) {
								double b;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
									double a;
									if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
										Stack[StackLength - 2] = a != b ? "1" : "0";
										StackLength--;
										q = false;
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "<":
						{
							bool q = true;
							if (StackLength >= 2) {
								double b;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
									double a;
									if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
										Stack[StackLength - 2] = a < b ? "1" : "0";
										StackLength--;
										q = false;
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case ">":
						{
							bool q = true;
							if (StackLength >= 2) {
								double b;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
									double a;
									if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
										Stack[StackLength - 2] = a > b ? "1" : "0";
										StackLength--;
										q = false;
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "<=":
						{
							bool q = true;
							if (StackLength >= 2) {
								double b;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
									double a;
									if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
										Stack[StackLength - 2] = a <= b ? "1" : "0";
										StackLength--;
										q = false;
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case ">=":
						{
							bool q = true;
							if (StackLength >= 2) {
								double b;
								if (double.TryParse(Stack[StackLength - 1], System.Globalization.NumberStyles.Float, Culture, out b)) {
									double a;
									if (double.TryParse(Stack[StackLength - 2], System.Globalization.NumberStyles.Float, Culture, out a)) {
										Stack[StackLength - 2] = a >= b ? "1" : "0";
										StackLength--;
										q = false;
									}
								}
							}
							if (q) {
								Stack[StackLength] = Arguments[i];
								StackLength++;
							}
						} break;
					case "floor":
						if (StackLength >= 1 && Stack[StackLength - 1] == "/") {
							Stack[StackLength - 1] = "quotient";
						} else {
							Stack[StackLength] = Arguments[i];
							StackLength++;
						} break;
					default:
						Stack[StackLength] = Arguments[i];
						StackLength++;
						break;
				}
			}
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			for (int i = 0; i < StackLength; i++) {
				if (i != 0) Builder.Append(' ');
				Builder.Append(Stack[i]);
			}
			return Builder.ToString();
		}

		/// <summary>Converts a string in infix function notation into simple function script</summary>
		/// <param name="Expression">The function script string</param>
		/// <param name="Preprocessing">Whether this is preprocessing</param>
		/// <returns>The simple function script string</returns>
		public static string GetFunctionNotationFromInfixNotation(string Expression, bool Preprocessing) {
			// brackets
			if (Preprocessing) {
				int s = 0;
				while (true) {
					if (s >= Expression.Length) break;
					int i = Expression.IndexOf('[', s);
					if (i >= s) {
						int j = i + 1, t = j, m = 1;
						string[] p = new string[4]; int n = 0;
						bool q = false;
						while (j < Expression.Length) {
							switch (Expression[j]) {
								case '[':
									m++;
									break;
								case ']':
									m--;
									if (m < 0) {
										throw new System.IO.InvalidDataException("Unexpected closing bracket encountered in " + Expression);
									}
									if (m == 0)
									{
										if (n >= p.Length) Array.Resize<string>(ref p, n << 1);
										p[n] = Expression.Substring(t, j - t);
										n++;
										string a = Expression.Substring(0, i).Trim();
										string c = Expression.Substring(j + 1).Trim();
										System.Text.StringBuilder r = new System.Text.StringBuilder();
										for (int k = 0; k < n; k++)
										{
											p[k] = GetFunctionNotationFromInfixNotation(p[k], true);
											if (k > 0) r.Append(',');
											r.Append(p[k]);
										}
										Expression = a + "[" + r + "]" + c;
										s = i + r.Length + 2;
										q = true;
									}
									break;
								case ',':
									if (m == 1) {
										if (n >= p.Length) Array.Resize<string>(ref p, n << 1);
										p[n] = Expression.Substring(t, j - t);
										n++;
										t = j + 1;
									}
									break;
							}
							if (q) {
								break;
							}
							j++;
						}
						if (!q) {
							throw new System.IO.InvalidDataException("Missing closing bracket encountered in " + Expression);
						}
					} else {
						break;
					}
				}
			}
			// parentheses
			{
				int i = Expression.IndexOf('(');
				if (i >= 0) {
					int j = i + 1;
					int n = 1;
					while (j < Expression.Length) {
						switch (Expression[j]) {
							case '(':
								n++;
								break;
							case ')':
								n--;
								if (n < 0) {
									throw new System.IO.InvalidDataException("Unexpected closing parenthesis encountered in " + Expression);
								}
								if (n == 0)
								{
									string a = Expression.Substring(0, i).Trim();
									string b = Expression.Substring(i + 1, j - i - 1).Trim();
									string c = Expression.Substring(j + 1).Trim();
									return GetFunctionNotationFromInfixNotation(a + GetFunctionNotationFromInfixNotation(b, false) + c,
										false);
								}
								break;
						} j++;
					}
					throw new System.IO.InvalidDataException("No closing parenthesis found in " + Expression);
				} else {
					i = Expression.IndexOf(')');
					if (i >= 0) {
						throw new System.IO.InvalidDataException("Unexpected closing parenthesis encountered in " + Expression);
					}
				}
			}
			// operators
			{
				int i = Expression.IndexOf('|');
				if (i >= 0) {
					string a = Expression.Substring(0, i).Trim();
					string b = Expression.Substring(i + 1).Trim();
					return "Or[" + GetFunctionNotationFromInfixNotation(a, false) + "," + GetFunctionNotationFromInfixNotation(b, false) + "]";
				}
			}
			{
				int i = Expression.IndexOf('^');
				if (i >= 0) {
					string a = Expression.Substring(0, i).Trim();
					string b = Expression.Substring(i + 1).Trim();
					return "Xor[" + GetFunctionNotationFromInfixNotation(a, false) + "," + GetFunctionNotationFromInfixNotation(b, false) + "]";
				}
			}
			{
				int i = Expression.IndexOf('&');
				if (i >= 0) {
					string a = Expression.Substring(0, i).Trim();
					string b = Expression.Substring(i + 1).Trim();
					return "And[" + GetFunctionNotationFromInfixNotation(a, false) + "," + GetFunctionNotationFromInfixNotation(b, false) + "]";
				}
			}
			{
				int i = Expression.IndexOf('!');
				while (true) {
					if (i >= 0) {
						if (i < Expression.Length - 1) {
							if (Expression[i + 1] == '=') {
								int j = Expression.IndexOf('!', i + 2);
								i = j < i + 2 ? -1 : j;
							} else break;
						} else break;
					} else break;
				}
				if (i >= 0) {
					string b = Expression.Substring(i + 1).Trim();
					return "Not[" + GetFunctionNotationFromInfixNotation(b, false) + "]";
				}
			}
			{
				int[] j = new int[6];
				j[0] = Expression.LastIndexOf("==", StringComparison.Ordinal);
				j[1] = Expression.LastIndexOf("!=", StringComparison.Ordinal);
				j[2] = Expression.LastIndexOf("<=", StringComparison.Ordinal);
				j[3] = Expression.LastIndexOf(">=", StringComparison.Ordinal);
				j[4] = Expression.LastIndexOf("<", StringComparison.Ordinal);
				j[5] = Expression.LastIndexOf(">", StringComparison.Ordinal);
				int k = -1;
				for (int i = 0; i < j.Length; i++) {
					if (j[i] >= 0) {
						if (k >= 0) {
							if (j[i] > j[k]) k = i;
						} else {
							k = i;
						}
					}
				}
				if (k >= 0) {
					int l = k <= 3 ? 2 : 1;
					string a = Expression.Substring(0, j[k]).Trim();
					string b = Expression.Substring(j[k] + l).Trim();
					string f; switch (k) {
							case 0: f = "Equal"; break;
							case 1: f = "Unequal"; break;
							case 2: f = "LessEqual"; break;
							case 3: f = "GreaterEqual"; break;
							case 4: f = "Less"; break;
							case 5: f = "Greater"; break;
							default: f = "Halt"; break;
					}
					return f + "[" + GetFunctionNotationFromInfixNotation(a, false) + "," + GetFunctionNotationFromInfixNotation(b, false) + "]";
				}
			}
			{
				int i = Expression.LastIndexOf('+');
				int j = Expression.LastIndexOf('-');
				if (i >= 0 & (j == -1 | j >= 0 & i > j)) {
					string a = Expression.Substring(0, i).Trim();
					string b = Expression.Substring(i + 1).Trim();
					return "Plus[" + GetFunctionNotationFromInfixNotation(a, false) + "," + GetFunctionNotationFromInfixNotation(b, false) + "]";
				} else if (j >= 0) {
					string a = Expression.Substring(0, j).Trim();
					string b = Expression.Substring(j + 1).Trim();
					if (a.Length != 0) {
						return "Subtract[" + GetFunctionNotationFromInfixNotation(a, false) + "," + GetFunctionNotationFromInfixNotation(b, false) + "]";
					}
				}
			}
			{
				int i = Expression.IndexOf('*');
				if (i >= 0) {
					string a = Expression.Substring(0, i).Trim();
					string b = Expression.Substring(i + 1).Trim();
					return "Times[" + GetFunctionNotationFromInfixNotation(a, false) + "," + GetFunctionNotationFromInfixNotation(b, false) + "]";
				}
			}
			{
				int i = Expression.IndexOf('/');
				if (i >= 0) {
					string a = Expression.Substring(0, i).Trim();
					string b = Expression.Substring(i + 1).Trim();
					return "Divide[" + GetFunctionNotationFromInfixNotation(a, false) + "," + GetFunctionNotationFromInfixNotation(b, false) + "]";
				}
			}
			{
				int i = Expression.IndexOf('-');
				if (i >= 0) {
					string a = Expression.Substring(0, i).Trim();
					string b = Expression.Substring(i + 1).Trim();
					if (a.Length == 0) {
						return "Minus[" + GetFunctionNotationFromInfixNotation(b, false) + "]";
					}
				}
			}
			return Expression.Trim();
		}
	}
}
