Feature: Fibonaci
  Scenario: Get fibonaci series
    Given fibonaci length 
      And output format
    When generate fibonaci series
    Then receive the fibonaci base on output format