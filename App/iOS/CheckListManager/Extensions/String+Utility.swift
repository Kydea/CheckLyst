//
//  String+Utility.swift
//  Checklyst
//
//  Created by Simone Testa on 10/08/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import Foundation

extension String {
    func replacingOccurrences(regex pattern : String, with: String) -> String {
        do {
            let regex = try NSRegularExpression(pattern: pattern, options: NSRegularExpression.Options.caseInsensitive)
            let range = NSMakeRange(0, self.count)
            return regex.stringByReplacingMatches(in: self, options: [], range: range, withTemplate: with)
        } catch {
            return self
        }
    }
}
