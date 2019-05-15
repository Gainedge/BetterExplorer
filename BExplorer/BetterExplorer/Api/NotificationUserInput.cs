// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BetterExplorer.Api {

  /// <summary>
  /// Text and selection values that the user entered on your notification. The Key is the ID of the input, and the Value is what the user entered.
  /// </summary>
  public class NotificationUserInput : IReadOnlyDictionary<string, string> {
    private NotificationActivator.NOTIFICATION_USER_INPUT_DATA[] _data;
    internal NotificationUserInput(NotificationActivator.NOTIFICATION_USER_INPUT_DATA[] data) {
      this._data = data;
    }
    public string this[string key] => this._data.First(i => i.Key == key).Value;
    public IEnumerable<string> Keys => this._data.Select(i => i.Key);
    public IEnumerable<string> Values => this._data.Select(i => i.Value);
    public int Count => this._data.Length;
    public bool ContainsKey(string key) {
      return this._data.Any(i => i.Key == key);
    }
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
      return this._data.Select(i => new KeyValuePair<string, string>(i.Key, i.Value)).GetEnumerator();
    }
    public bool TryGetValue(string key, out string value) {
      foreach (var item in this._data) {
        if (item.Key == key) {
          value = item.Value;
          return true;
        }
      }

      value = null;
      return false;
    }
    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }
  }
}
